using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features.Hospital;
using WebAPI.Extensions;
using Application.Abstractions;
using Infrastructure.Services.SEIH;


namespace WebAPI.Controllers.SEIH.HOSPITALMANAGER;

[ApiController]
[Route("api/test")]
public class TestConnectionController : ControllerBase
{
    private readonly ISeihTransferClient _client;

    public TestConnectionController(ISeihTransferClient client)
    {
        _client = client;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("ping-transfert", Name = "TestConnection")]
    public async Task<IActionResult> Ping()
    {
        var response = await _client.PingAsync(); 
        return Ok(response);
    }
}