using Infrastructure.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers.SEIH.Transfer;

[ApiController]
[Route("seih/external/hospital")]
[AllowAnonymous] // 👈 appliqué à tout le controller
public class ExternalHospitalController : ControllerBase
{
    private readonly TransfertApiClient _transfertApiClient;

    public ExternalHospitalController(
        TransfertApiClient transfertApiClient)
    {
        _transfertApiClient = transfertApiClient;
    }

    [HttpPost("list")] // 👈 POST au lieu de GET
    [AllowAnonymous]
    public async Task<IActionResult> GetExternalHospitals()
    {
        var result = await _transfertApiClient.GetHospitalsAsync();
        return Ok(result);
    }
}