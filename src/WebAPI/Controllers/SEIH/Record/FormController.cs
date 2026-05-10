using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.User;
using Core.Application.Model.Features.Record;
using Core.Application.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Base;
using Core.Application.Interface.Services.SEIH.Record;

namespace WebAPI.Controllers.SEIH.Record;

[ApiController]
[Route("seih/form")]
public class FormController : BaseController
{
    private readonly IFormService _service;

    public FormController(IFormService service)
    {
        _service = service;
    }

    // CREATE
    [HttpPost]
    [AllowAnonymous]
    [Route("create", Name = "FormCreate")]
    public async Task<IActionResult> CreateFormAsync(CreateFormDto model)
    {
        var result = await _service.CreateAsync(User, model);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // UPDATE
    [HttpPost]
    [AllowAnonymous]
    [Route("update", Name = "FormUpdate")]
    public async Task<IActionResult> UpdateFormAsync(UpdateFormDto model)
    {
        var result = await _service.UpdateAsync(User, model);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // GET LIST
    [HttpPost]
    [AllowAnonymous]
    [Route("get/list", Name = "FormList")]
    public async Task<IActionResult> GetFormListAsync()
    {
        var result = await _service.GetByHospitalAsync(User);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // GET BY ID
    [HttpPost]
    [AllowAnonymous]
    [Route("get/by-id", Name = "FormGetById")]
    public async Task<IActionResult> GetFormByIdAsync(GetByIdDto model)
    {
        var result = await _service.GetByIdAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // DELETE
    [HttpPost]
    [AllowAnonymous]
    [Route("delete", Name = "FormDelete")]
    public async Task<IActionResult> DeleteFormAsync(GetByIdDto model)
    {
        var result = await _service.DeleteAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }
}