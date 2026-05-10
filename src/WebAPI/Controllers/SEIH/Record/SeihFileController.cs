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
[Route("seih/file")]
public class SeihFileController : BaseController
{
    private readonly ISeihFileService _service;

    public SeihFileController(ISeihFileService service)
    {
        _service = service;
    }

    // ============================
    // UPLOAD
    // ============================
    [HttpPost]
    [AllowAnonymous]
    [Route("upload", Name = "SeihFileUpload")]
    public async Task<IActionResult> UploadAsync([FromForm] UploadFileRequestDto model)
    {
        var result = await _service.UploadAsync(User, model);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }

    // ============================
    // DOWNLOAD
    // ============================
    [HttpPost]
    [AllowAnonymous]
    [Route("download", Name = "SeihFileDownload")]
    public async Task<IActionResult> DownloadAsync(GetByIdDto model)
    {
        var result = await _service.DownloadAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return File(
            result.Result.FileBytes,
            result.Result.ContentType,
            result.Result.FileName
        );
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("preview", Name = "SeihFilePreview")]
    public async Task<IActionResult> PreviewAsync(GetByIdDto model)
    {
        var result = await _service.DownloadAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return File(
            result.Result.FileBytes,
            result.Result.ContentType
        );
    }

    // ============================
    // DELETE (soft delete)
    // ============================
    [HttpPost]
    [AllowAnonymous]
    [Route("delete", Name = "SeihFileDelete")]
    public async Task<IActionResult> DeleteAsync(GetByIdDto model)
    {
        var result = await _service.DeleteAsync(User, model.Id);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }
}