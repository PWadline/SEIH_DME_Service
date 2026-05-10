using AutoMapper;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using Core.Application.Model.Request;
using Core.Domain.Entity.SEIH;
using Core.Domain.Procedures.SEIH;
using Infrastructure.Repository.SEIH.Hospital;
using Infrastructure.Repository.SEIH.User;
using Infrastructure.Security;
using Infrastructure.Utils;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace Infrastructure.Services.SEIH.Hospital;

public class HospitalUserService : IHospitalUserService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IHospitalRoleRepository _hospitalRoleRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IMapper _mapper;
    public HospitalUserService(IUsersRepository usersRepository, IMapper mapper, IHospitalRepository hospitalRepository,
        IHospitalRoleRepository hospitalRoleRepository, IRolesRepository rolesRepository)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _hospitalRepository = hospitalRepository;
        _hospitalRoleRepository = hospitalRoleRepository;
        _rolesRepository = rolesRepository;
    }
    public async Task<ServiceResult<bool>> HospitalAddRoleToUserServiceAsync(ClaimsPrincipal claim, AddRolesToUserDTO dataModel)
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

        var roleCreation = await _hospitalRoleRepository.GetRoleByNameAsync(dataModel.RoleName!, manager.HospitalId);
        if (roleCreation == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        var userId = Guid.Parse(dataModel.UserId!);
        var isUserExists = await _usersRepository.GetUserByIdAsync(userId);

        if (isUserExists == null)
        {
            return new ServiceResult<bool>(System.Net.HttpStatusCode.NotAcceptable);
        }

        var IsRoleAssigned = await _rolesRepository.AssignRoles(isUserExists!.Id, roleCreation.Id);

        if (!IsRoleAssigned)
        {
            return new ServiceResult<bool>(System.Net.HttpStatusCode.NotAcceptable);
        }
        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<string>> HospitalCreateUserServiceAsync(ClaimsPrincipal claim, CreateUserModel dataModel)
    {
        var email = claim.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);
        if (manager == null)
        {
            return new ServiceResult<string>(HttpStatusCode.Unauthorized);
        }

        var isUserExists = await _usersRepository.GetUserByEmailAsync(dataModel.Email!);
        if (isUserExists != null)
        {
            return new ServiceResult<string>(HttpStatusCode.NotAcceptable);
        }

        var user = _mapper.Map<UsersEntity>(dataModel);

        user.Id = Guid.NewGuid();

        user.Username = UsernameGenerator.CreateUsername(
            dataModel.FirstName!,
            dataModel.LastName!
        );

        user.Email = dataModel.Email!.ToLower();

        var generatedPassword = $"Seih_{user.FirstName}@2026";

        user.Salt = PasswordSalt.GenerateSalt();
        user.PasswordHash = MyPasswordHasher.HashPassword(user, generatedPassword);

        user.IsNewPasswordRequired = true;

        user.HospitalId = manager.HospitalId;

        var userCreation = await _usersRepository.CreateUserAsync(user);

        if (!userCreation)
        {
            return new ServiceResult<string>(HttpStatusCode.InternalServerError);
        }
        if (dataModel.RoleId.HasValue)
        {
            Console.WriteLine("🔥 RoleId reçu: " + dataModel.RoleId);

            var role = await _rolesRepository.GetRoleById(dataModel.RoleId.Value); // ⭐ FIX ICI

            if (role == null)
            {
                return new ServiceResult<string>(HttpStatusCode.NotAcceptable);
            }

            if (role.HospitalId != manager.HospitalId)
            {
                return new ServiceResult<string>(HttpStatusCode.Unauthorized);
            }

            var assign = await _rolesRepository.AssignRoles(user.Id, role.Id);

            if (!assign)
            {
                return new ServiceResult<string>(HttpStatusCode.InternalServerError);
            }
        }
        else
        {
            Console.WriteLine("⚠️ Aucun RoleId envoyé");
        }

        return new ServiceResult<string>(generatedPassword);
    }



    public async Task<ServiceResult<bool>> HospitalUpdateUserPasswordServiceAsync(ClaimsPrincipal claim, ChangePasswordModel dataModel)
    {
        var email = claim.Claims
                      .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
                      .Select(c => c.Value)
                      .FirstOrDefault();

        var user = await _usersRepository.GetUserByEmailAsync(email!);
        if (user == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        if (!MyPasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, dataModel.OldPassword!))
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        user.IsNewPasswordRequired = false;
        user.Salt = PasswordSalt.GenerateSalt();
        user.PasswordHash = MyPasswordHasher.HashPassword(user, dataModel.NewPassword!);
        user.LastModified = DateTime.UtcNow;
        user.LastModifiedBy = user.Id.ToString();
        var updateResult = await _usersRepository.UpdateUserAsync(user);

        if (!updateResult)
        {
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<bool>> HospitalUpdateUserPasswordByManagerServiceAsync(ClaimsPrincipal claim, ChangePasswordByManagerModel dataModel)
    {
        var email = claim.Claims
                      .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
                      .Select(c => c.Value)
                      .FirstOrDefault();

        var user = await _usersRepository.GetUserByEmailAsync(email!);
        if (user == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }
        var targetUser = await _usersRepository.GetUserByEmailAsync(dataModel.UserEmail!);
        if (targetUser == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        targetUser.IsNewPasswordRequired = true;
        targetUser.Salt = PasswordSalt.GenerateSalt();
        targetUser.PasswordHash = MyPasswordHasher.HashPassword(targetUser, dataModel.NewPassword!);
        targetUser.LastModified = DateTime.UtcNow;
        targetUser.LastModifiedBy = user.Id.ToString();
        var updateResult = await _usersRepository.UpdateUserAsync(targetUser);

        if (!updateResult)
        {
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<bool>> HospitalUpdateUserServiceAsync(ClaimsPrincipal claim, CreateUserModel dataModel)
    {
        var email = claim.Claims
              .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
              .Select(c => c.Value)
              .FirstOrDefault();

        var user = await _usersRepository.GetUserByEmailAsync(email!);
        if (user == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }
        var targetUser = await _usersRepository.GetUserByEmailAsync(dataModel.Email!);
        if (targetUser == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        targetUser.FirstName = dataModel.FirstName;
        targetUser.LastName = dataModel.LastName;
        targetUser.Email = dataModel.Email!.ToLower();
        targetUser.LastModified = DateTime.UtcNow;
        targetUser.LastModifiedBy = user.Id.ToString();
        var updateResult = await _usersRepository.UpdateUserAsync(targetUser);

        if (!updateResult)
        {
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<IEnumerable<GetUserListWithRolesResponse>>> HospitalGetUsersListWithRolesAsync(ClaimsPrincipal claim)
    {
        var email = claim.Claims
               .Where(c => c.Type == System.Security.Claims.ClaimTypes.Email)
               .Select(c => c.Value)
               .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);
        if (manager == null)
        {
            return new ServiceResult<IEnumerable<GetUserListWithRolesResponse>>(HttpStatusCode.Unauthorized);
        }

        var users = await _usersRepository.GetAllHospitalUsersWithRolesAsync(manager.HospitalId!);

        return new ServiceResult<IEnumerable<GetUserListWithRolesResponse>>(users);
    }

   
   
    public async Task<ServiceResult<bool>> HospitalUpdateUserProfileAsync(
      ClaimsPrincipal claim,
      UpdateUserProfileModel model)
    {
        var email = claim.Claims
            .Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);
        if (manager == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByIdAsync(model.UserId);
        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.NotAcceptable);

        // 🔒 1. ACTIVER / DÉSACTIVER
        if (model.IsActive.HasValue)
        {
            user.IsActive = model.IsActive.Value;
        }

        // 🔐 2. CHANGER MOT DE PASSE (SEULEMENT SI FOURNI)
        if (!string.IsNullOrEmpty(model.NewPassword?.Trim()))
        {
            user.Salt = PasswordSalt.GenerateSalt();
            user.PasswordHash = MyPasswordHasher.HashPassword(user, model.NewPassword.Trim());
            user.IsNewPasswordRequired = true;
        }

        // 🎭 3. GESTION ROLE UNIQUE
        if (model.RoleId.HasValue)
        {
            var role = await _rolesRepository.GetRoleById(model.RoleId.Value);

            if (role == null)
                return new ServiceResult<bool>(HttpStatusCode.NotAcceptable);

            if (role.HospitalId != manager.HospitalId)
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

            // ❌ supprimer tous les anciens rôles
            await _rolesRepository.RemoveAllUserRoles(user.Id);

            // ✅ assigner le nouveau rôle
            var assign = await _rolesRepository.AssignRoles(user.Id, role.Id);

            if (!assign)
                return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        // 📝 audit
        user.LastModified = DateTime.UtcNow;
        user.LastModifiedBy = manager.Id.ToString();

        var update = await _usersRepository.UpdateUserAsync(user);

        if (!update)
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);

        return new ServiceResult<bool>(true);
    }





}
