using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.External; // TransfertApiClient
using Core.DTOs.External;
using Core.Application.Model.Features;
using Core.Application.Interface.Services.SEIH.Hospital;

namespace WebAPI.Controllers.SEIH.Transfer;

[ApiController]
[AllowAnonymous]
[Route("dme/transfer-request")]
public class TransferRequestController : ControllerBase
{
    private readonly ITransferRequestService _service;


    public TransferRequestController(ITransferRequestService service)
    {
        _service = service;
    }


    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransferRequestDto dto)
    {
        var result = await _service.CreateAsync(User, dto);

        if (result.IsError)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }


    [HttpPost("respond")]
    [AllowAnonymous]
    public async Task<IActionResult> Respond(
        [FromBody] RespondTransferRequestDto dto)
    {
        var result = await _service.RespondAsync(User, dto);

        if (result.IsError)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }


    [HttpPost("list")]
    [AllowAnonymous]
    public async Task<IActionResult> List()
    {
        var result = await _service.GetMyRequestsAsync(User);

        if (result.IsError)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }

}