using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.External;
using Core.DTOs.External;
using Core.Application.Model.Features;
using Core.Application.Interface.Services.SEIH.Hospital;

namespace WebAPI.Controllers.SEIH.Transfer;

[ApiController]
[AllowAnonymous]
[Route("seih/transfers")]
public class TransferController : ControllerBase
{
    private readonly ITransferService _transferService;
    private readonly IInboundTransferService _inboundTransferService;

    public TransferController(ITransferService transferService,
    IInboundTransferService inboundTransferService)
    {
        _transferService = transferService;
        _inboundTransferService = inboundTransferService;
    }

    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateTransferDto request)
    {
        var result = await _transferService.CreateTransferAsync(
            User,
            request
        );

        return StatusCode((int)result.Status, result);
    }

    [HttpPost("list")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTransfers()
    {
        var result = await _transferService.GetTransferListAsync(User);
        return StatusCode((int)result.Status, result);
    }

    [HttpPost("pull")]
    [AllowAnonymous]
    public async Task<IActionResult> PullTransfers([FromBody] Guid hospitalId)
    {
        await _inboundTransferService.PullIncomingTransfersAsync(hospitalId);

        return Ok("Transfers processed");
    }
}