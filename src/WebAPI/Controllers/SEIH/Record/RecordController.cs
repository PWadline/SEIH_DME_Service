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
[Route("seih/record")]
public class RecordController : BaseController
{
    private readonly IRecordService _service;
    private readonly ILogger<RecordController> _logger;
    public RecordController(
        IRecordService service,
        ILogger<RecordController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [Route("create", Name = "RecordCreate")]
    public async Task<IActionResult> CreateRecordAsync(
    [FromForm] CreateRecordDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Model HospitalBranchId reçu = {branchId}", model.HospitalBranchId);
            var result = await _service.CreateAsync(User, model);

            if (result.IsError)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("update", Name = "RecordUpdate")]
    public async Task<IActionResult> UpdateRecordAsync(
    [FromForm] UpdateRecordDto model)
    {
        if (model == null)
            return BadRequest("Request body is null.");

        if (model.Id == Guid.Empty)
            return BadRequest("Invalid record Id.");

        if (model.FieldValues == null || !model.FieldValues.Any())
            return BadRequest("FieldValues are required.");

        var result = await _service.UpdateAsync(User, model);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("get/list", Name = "RecordList")]
    public async Task<IActionResult> GetRecordListAsync()
    {
        var result = await _service.GetByHospitalAsync(User);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("get/by-id", Name = "RecordGetById")]
    public async Task<IActionResult> GetRecordByIdAsync(GetByIdDto model)
    {
        var result = await _service.GetByIdAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("delete", Name = "RecordDelete")]
    public async Task<IActionResult> DeleteRecordAsync(GetByIdDto model)
    {
        var result = await _service.DeleteAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }
    
    [HttpPost("reset-code")]
    public async Task<IActionResult> ResetCode([FromBody] ResetPatientCodeDto dto)
    {
        var result = await _service.ResetPatientCodeAsync(dto);

        if (result.IsError)
            return BadRequest();

        return Ok(result.Result);
    }


    [HttpPost]
    [AllowAnonymous]
    [Route("consent/hospitals", Name = "GetAuthorizedHospitals")]
    public async Task<IActionResult> GetAuthorizedHospitalsAsync(GetAuthorizedHospitalsDto model)
    {
        if (model == null || model.RecordId == Guid.Empty)
            return BadRequest("RecordId invalide");

        var result = await _service.GetAuthorizedHospitalsAsync(model.RecordId);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result.Result);
    }
}