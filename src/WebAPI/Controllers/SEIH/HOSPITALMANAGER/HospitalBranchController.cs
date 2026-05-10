using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features.Hospital;
using WebAPI.Extensions;
using WebApi.Controllers.Base;

namespace WebAPI.Controllers.SEIH.HOSPITALMANAGER;

[ApiController]
[Route("seih/hospitalbranch")]
public class HospitalBranchController : BaseController
{
    private readonly IHospitalBranchService _service;

    public HospitalBranchController(IHospitalBranchService service)
    {
        _service = service;
    }

    // 🔹 GET ALL BY HOSPITAL (mine)
    [HttpPost]
    [AllowAnonymous]
    [Route("get/byhospital", Name = "HospitalBranchGetByHospital")]
    public async Task<IActionResult> GetAllByHospitalAsync()
    {
        var result = await _service.GetAllByHospitalAsync(User);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }

    // 🔹 GET BY ID
    [HttpPost]
    [AllowAnonymous]
    [Route("get/byid", Name = "HospitalBranchGetById")]
    public async Task<IActionResult> GetByIdAsync([FromBody] Guid id)
    {
        var result = await _service.GetByIdAsync(User, id);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }

    // 🔹 CREATE
    [HttpPost]
    [AllowAnonymous]
    [Route("create", Name = "HospitalBranchCreate")]
    public async Task<IActionResult> CreateAsync([FromForm] CreateHospitalBranchDto dto)
    {
        var result = await _service.CreateAsync(User, dto);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }

    // 🔹 UPDATE
    [HttpPost]
    [AllowAnonymous]
    [Route("update", Name = "HospitalBranchUpdate")]
    public async Task<IActionResult> UpdateAsync([FromForm] UpdateHospitalBranchDto dto)
    {
        var result = await _service.UpdateAsync(User, dto);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }

    // 🔹 DELETE
    [HttpPost]
    [AllowAnonymous]
    [Route("delete", Name = "HospitalBranchDelete")]
    public async Task<IActionResult> DeleteAsync([FromBody] Guid id)
    {
        var result = await _service.DeleteAsync(User, id);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }

    // 🔹 GET MINE
    [HttpPost]
    [AllowAnonymous]
    [Route("get/mine", Name = "HospitalBranchGetMine")]
    public async Task<IActionResult> GetMineAsync()
    {
        var result = await _service.GetAllByHospitalAsync(User);

        if (result.IsError)
            return StatusCode((int)result.Status);

        return Ok(result);
    }
}