using AutoMapper;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH.User;
using Core.Application.Model.Features.Hospital;
using Core.Application.Model.Request;
using Core.Domain.Entities;
using Core.Domain.Entity.SEIH;
using Infrastructure.Security;
using Infrastructure.Utils;
using System.Net;
using System.Security.Claims;

namespace Infrastructure.Services.SEIH.User
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRolesRepository _rolesRepository;
        private readonly IHospitalRepository _hospitalRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUsersRepository usersRepository,
            IRolesRepository rolesRepository,
            IMapper mapper,
            AppDbContext applicationDbContext,
            IHospitalRepository hospitalRepository)
        {
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
            _mapper = mapper;
            _hospitalRepository = hospitalRepository;
        }

        public async Task<ServiceResult<bool>> SEIH_AddRolesToUserAsync(ClaimsPrincipal claim, AddRolesToUserDto dataModel)
        {
            var email = claim.Claims
               .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
               .Select(c => c.Value)
               .FirstOrDefault();

            var manager = await _usersRepository.GetUserByEmailAsync(email!);
            if (manager == null)
            {
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
            }
            var user = await _usersRepository.GetUserByEmailAsync(dataModel.UserEmail!);
            if (user == null)
            {
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
            }


            var hospitalRole = await _rolesRepository.GetHospitalRole(manager.HospitalId, dataModel.RoleName!);
            if (hospitalRole == null)
            {
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
            }

            var existingRoles = await _rolesRepository.GetUserRole((Guid)user.Id!, (Guid)hospitalRole.Id!);
            if (existingRoles != null)
            {
                return new ServiceResult<bool>(HttpStatusCode.NotAcceptable);
            }

            var assignRole = await _rolesRepository.AssignRoles(user.Id, hospitalRole.Id);

            if (!assignRole)
            {
                return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
            }
            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> SEIH_CreateUserAsync(ClaimsPrincipal claim, CreateUserModel dataModel)
        {
            var email = claim.Claims
                          .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
                          .Select(c => c.Value)
                          .FirstOrDefault();

            var manager = await _usersRepository.GetUserByEmailAsync(email!);
            if (manager == null)
            {
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
            }

            var user = _mapper.Map<UsersEntity>(dataModel);
            var isUserExists = await _usersRepository.GetUserByEmailAsync(dataModel.Email!);

            if (isUserExists != null)
            {
                return new ServiceResult<bool>(System.Net.HttpStatusCode.NotAcceptable);
            }

            user.Id = Guid.NewGuid();

            var hospital = await _hospitalRepository.GetHospitalByIdAsync(manager.HospitalId);

            if (hospital == null)
            {
                return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
            }

            user.Username = (hospital.Code ?? "hosp").ToLower() + "-" +
                UsernameGenerator.CreateUsername(dataModel.FirstName!, dataModel.LastName!);


            user.Email = dataModel.Email!.ToLower();

            var generatedPassword = $"Seih_{user.FirstName}@2026";

            user.Salt = PasswordSalt.GenerateSalt();
            user.PasswordHash = MyPasswordHasher.HashPassword(user, generatedPassword);

            user.IsNewPasswordRequired = true;

            user.HospitalId = manager.HospitalId;

            var userCreation = await _usersRepository.CreateUserAsync(user);


            if (!userCreation)
            {
                return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
            }

            Console.WriteLine("🚀 Début assignation rôle");

            if (dataModel.RoleId.HasValue)
            {
                Console.WriteLine("✅ RoleId reçu: " + dataModel.RoleId);

                var role = await _rolesRepository.GetRoleById(dataModel.RoleId.Value);

                if (role == null)
                {
                    Console.WriteLine("❌ ROLE NULL");
                    return new ServiceResult<bool>(HttpStatusCode.NotAcceptable);
                }

                Console.WriteLine("✅ Role trouvé: " + role.Id);

                Console.WriteLine("Manager hospital: " + manager.HospitalId);
                Console.WriteLine("Role hospital: " + role.HospitalId);

                if (role.HospitalId != manager.HospitalId)
                {
                    Console.WriteLine("❌ HOSPITAL MISMATCH");
                    return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
                }

                var assign = await _rolesRepository.AssignRoles(user.Id, role.Id);

                Console.WriteLine("Assign result: " + assign);

                if (!assign)
                {
                    Console.WriteLine("❌ ASSIGN FAILED");
                    return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                Console.WriteLine("⚠️ RoleId NULL");
            }

            return new ServiceResult<bool>(true);
        }

    }
}
