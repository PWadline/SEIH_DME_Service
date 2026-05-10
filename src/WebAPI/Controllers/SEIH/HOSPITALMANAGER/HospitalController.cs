using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features.Hospital;
using WebAPI.Extensions;
using Core.Application.Model.Features;


namespace WebAPI.Controllers.SEIH.HOSPITALMANAGER;

[AllowAnonymous]
[Route("api/hospital")]
[ApiController]
public class HospitalController : ControllerBase
{
    private readonly IHospitalService _hospitalService;

    public HospitalController(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<IActionResult> Generate()
    {
        var result = await _hospitalService.GenerateAsync(User);
        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirm(ConfirmKeyRequestDto request)
    {
        var result = await _hospitalService.ConfirmAsync(User, request.PublicKey);
        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("key/status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetKeyStatus()
    {
        var result = await _hospitalService.GetKeyStatusAsync(User);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

}
