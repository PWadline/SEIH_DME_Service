using System.Security.Claims;

namespace WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetHospitalId(this ClaimsPrincipal user)
    {
        var hospitalIdClaim = user.FindFirst("HospitalId")?.Value;

        if (string.IsNullOrWhiteSpace(hospitalIdClaim))
            return Guid.Empty;

        return Guid.TryParse(hospitalIdClaim, out var hospitalId)
            ? hospitalId
            : Guid.Empty;
    }
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : Guid.Empty;
    }
}