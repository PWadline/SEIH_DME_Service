using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features.Hospital;
using WebAPI.Extensions;


namespace WebAPI.Controllers.SEIH.Sync;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly IHospitalSyncService _sync;

    public SyncController(IHospitalSyncService sync)
    {
        _sync = sync;
    }

    [HttpPost("hospitals")]
    public async Task<IActionResult> SyncHospitals()
    {
        await _sync.SyncAsync();
        return Ok("Hospitals synchronized.");
    }
}