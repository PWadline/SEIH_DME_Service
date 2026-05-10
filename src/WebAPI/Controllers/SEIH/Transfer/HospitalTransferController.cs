// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using Infrastructure.External;
// using Core.DTOs.External;
// using Core.Application.Model.Features;

// namespace WebAPI.Controllers.SEIH.Transfer;

// [ApiController]
// [Route("dme/transfer")]
// [AllowAnonymous]
// public class HospitalTransferController : ControllerBase
// {
//     private readonly TransfertApiClient _transfertApiClient;

//     public HospitalTransferController(
//         TransfertApiClient transfertApiClient)
//     {
//         _transfertApiClient = transfertApiClient;
//     }

//     // 🔁 CREATE TRANSFER
//     [HttpPost("create")]
//     public async Task<IActionResult> Create([FromBody] TransferReceiveDto request)
//     {
//         var result = await _transfertApiClient.CreateTransferAsync(request);
//         return Ok(result);
//     }

//     [HttpPost("list")]
//     public async Task<IActionResult> GetList([FromBody] TransferListRequest request)
//     {
//         var result = await _transfertApiClient
//             .GetTransferListAsync(request.HospitalId);

//         return Ok(result);
//     }
// }