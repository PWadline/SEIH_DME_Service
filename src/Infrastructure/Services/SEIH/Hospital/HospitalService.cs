using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features;
using Core.Domain.Entity.SEIH;
using System.Net;
using Core.Application.Interface.Repository.SEIH.Hospital;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Application.Model.Features.Hospital;
using Application.Abstractions;
using Infrastructure.Services.SEIH;


namespace Infrastructure.Services.SEIH;

public class HospitalService : IHospitalService
{
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ITransferInstitutionKeyRepository _transferInstitutionKeyRepository;
    private readonly ISeihTransferClient _client;

    public HospitalService(
    IHospitalRepository hospitalRepository,
    IUsersRepository usersRepository,
    ITransferInstitutionKeyRepository transferInstitutionKeyRepository,
    ISeihTransferClient client)
    {
        _hospitalRepository = hospitalRepository;
        _usersRepository = usersRepository;
        _transferInstitutionKeyRepository = transferInstitutionKeyRepository;
        _client = client;
    }

    public async Task<ServiceResult<HospitalDto>> CreateAsync(CreateHospitalDto dto)
    {
        var entity = new HospitalEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Address = dto.Address,
            City = dto.City,
            Department = dto.Department,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Created = DateTime.UtcNow,
            IsDeleted = false
        };

        await _hospitalRepository.AddAsync(entity);

        return new ServiceResult<HospitalDto>(new HospitalDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Address = entity.Address,
            City = entity.City,
            Department = entity.Department,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber
        });
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var entity = await _hospitalRepository.GetByIdAsync(id);

        if (entity == null)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        entity.IsDeleted = true;
        entity.LastModified = DateTime.UtcNow;

        await _hospitalRepository.UpdateAsync(entity);

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<List<HospitalDto>>> GetAllAsync()
    {
        var hospitals = await _hospitalRepository.GetAllAsync();

        var result = hospitals
            .Where(h => h.IsDeleted != true)
            .Select(h => new HospitalDto
            {
                Id = h.Id,
                Name = h.Name,
                Code = h.Code,
                Address = h.Address,
                City = h.City,
                Department = h.Department,
                Email = h.Email,
                PhoneNumber = h.PhoneNumber
            })
            .ToList();

        return new ServiceResult<List<HospitalDto>>(result);
    }

    public async Task<ServiceResult<HospitalDto?>> GetByIdAsync(Guid id)
    {
        var entity = await _hospitalRepository.GetByIdAsync(id);

        if (entity == null || entity.IsDeleted == true)
            return new ServiceResult<HospitalDto?>(HttpStatusCode.NotFound);

        var dto = new HospitalDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Address = entity.Address,
            City = entity.City,
            Department = entity.Department,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber
        };

        return new ServiceResult<HospitalDto?>(dto);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(UpdateHospitalDto dto)
    {
        var entity = await _hospitalRepository.GetByIdAsync(dto.Id);

        if (entity == null || entity.IsDeleted == true)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Department = dto.Department;
        entity.Email = dto.Email;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.LastModified = DateTime.UtcNow;

        await _hospitalRepository.UpdateAsync(entity);

        return new ServiceResult<bool>(true);
    }

    public (string publicKeyPem, string privateKeyPem, string fingerprint) Generate()
    {
        using var rsa = RSA.Create(2048);

        var privateKey = rsa.ExportPkcs8PrivateKey();
        var publicKey = rsa.ExportSubjectPublicKeyInfo();

        var privatePem = ExportPrivateKeyPem(privateKey);
        var publicPem = ExportPublicKeyPem(publicKey);

        var fingerprint = ComputeFingerprint(publicKey);

        return (publicPem, privatePem, fingerprint);
    }

    private string ExportPrivateKeyPem(byte[] key)
    {
        var base64 = Convert.ToBase64String(key);
        var builder = new StringBuilder();
        builder.AppendLine("-----BEGIN PRIVATE KEY-----");

        for (int i = 0; i < base64.Length; i += 64)
            builder.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));

        builder.AppendLine("-----END PRIVATE KEY-----");

        return builder.ToString();
    }

    private string ExportPublicKeyPem(byte[] key)
    {
        var base64 = Convert.ToBase64String(key);
        var builder = new StringBuilder();
        builder.AppendLine("-----BEGIN PUBLIC KEY-----");

        for (int i = 0; i < base64.Length; i += 64)
            builder.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));

        builder.AppendLine("-----END PUBLIC KEY-----");

        return builder.ToString();
    }

    private string ComputeFingerprint(byte[] publicKey)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(publicKey);
        return Convert.ToHexString(hash);
    }

    public async Task<ServiceResult<KeyGenerationDto>> GenerateAsync(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return new ServiceResult<KeyGenerationDto>(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByEmailAsync(email);
        if (user == null)
            return new ServiceResult<KeyGenerationDto>(HttpStatusCode.Unauthorized);

        var (publicPem, privatePem, fingerprint) = Generate();

        return new ServiceResult<KeyGenerationDto>(new KeyGenerationDto
        {
            PublicKey = publicPem,
            PrivateKey = privatePem,
            Fingerprint = fingerprint,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<ServiceResult> ConfirmAsync(ClaimsPrincipal claim, string publicKey)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return new ServiceResult(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByEmailAsync(email);
        if (user == null)
            return new ServiceResult(HttpStatusCode.Unauthorized);

        var existingKeys = await _transferInstitutionKeyRepository
    .GetByHospitalAsync(user.HospitalId);

        foreach (var k in existingKeys)
        {
            k.IsActive = false;
            await _transferInstitutionKeyRepository.UpdateAsync(k);
        }

        var nextVersion = await GetNextVersion(user.HospitalId);

        var cleanKey = publicKey
            .Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(" ", "")
            .Trim();

        var publicKeyBytes = Convert.FromBase64String(cleanKey);

        var fingerprint = ComputeFingerprint(publicKeyBytes);

        var key = new TransferInstitutionKeyEntity
        {
            Id = Guid.NewGuid(),
            HospitalId = user.HospitalId,
            PublicKey = publicKey,
            KeyVersion = nextVersion,
            Created = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(2),
            Fingerprint = fingerprint,
            IsActive = true
        };

        await _transferInstitutionKeyRepository.AddAsync(key);

        var dto = new RegisterTransferKeyRequest
        {
            HospitalId = user.HospitalId,
            PublicKey = publicKey,
            KeyVersion = nextVersion
        };

        await _client.RegisterTransferKeyAsync(dto);

        return new ServiceResult(HttpStatusCode.OK);
    }

    private async Task<int> GetNextVersion(Guid hospitalId)
    {
        var keys = await _transferInstitutionKeyRepository.GetByHospitalAsync(hospitalId);

        if (!keys.Any())
            return 1;

        return keys.Max(k => k.KeyVersion) + 1;
    }

    public async Task<ServiceResult<KeyStatusDto>> GetKeyStatusAsync(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return new ServiceResult<KeyStatusDto>(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByEmailAsync(email);

        if (user == null)
            return new ServiceResult<KeyStatusDto>(HttpStatusCode.Unauthorized);

        var keys = await _transferInstitutionKeyRepository
            .GetByHospitalAsync(user.HospitalId);

        if (keys == null || !keys.Any())
        {
            return new ServiceResult<KeyStatusDto>(new KeyStatusDto
            {
                HasKey = false
            });
        }

        var activeKey = keys
            .Where(k => k.IsActive == true)
            .OrderByDescending(k => k.KeyVersion)
            .FirstOrDefault();

        if (activeKey == null)
        {
            return new ServiceResult<KeyStatusDto>(new KeyStatusDto
            {
                HasKey = false
            });
        }

        return new ServiceResult<KeyStatusDto>(new KeyStatusDto
        {
            HasKey = true,
            Fingerprint = activeKey.Fingerprint,
            CreatedAt = activeKey.Created,
            ExpirationDate = activeKey.ExpirationDate,
            KeyVersion = activeKey.KeyVersion
        });
    }

}








