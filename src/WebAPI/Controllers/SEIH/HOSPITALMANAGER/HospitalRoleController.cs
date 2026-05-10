using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.User;
using Core.Application.Model.Features.Hospital;
using Core.Application.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Base;

namespace WebAPI.Controllers.SEIH.HOSPITALMANAGER;

[ApiController]
[Route("seih/hospital/role")] 
public class HospitalRoleController : BaseController
{
    private readonly IHospitalRoleService _service;

    public HospitalRoleController(IHospitalRoleService service)
    {
        _service = service;
    }


    [HttpPost]
    [AllowAnonymous]
    [Route("create", Name = "HospitalRoleCreate")]
    public async Task<IActionResult> CreateHospitalRoleAsync(HospitalRoleDto model)
    {
        var result = await _service.HospitalCreateRoleServiceAsync(User, model);

        if (result.IsError)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }


    [HttpPost]
    [AllowAnonymous]
    [Route("get/list", Name = "RoleList")]
    public async Task<IActionResult> GetRoleListAsync()
    {
        var result = await _service.GetAllRoleServiceAsync(User);

        if (result.IsError)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }


    [HttpPost]
    [Route("update", Name = "UpdateRole")]
    public async Task<IActionResult> UpdateRoleAsync(UpdateRoleDto model)
    {
        var result = await _service.UpdateRoleServiceAsync(User, model);

        if (result.IsError)
            return BadRequest(result);

        return Ok(result);
    }
[HttpPost]
[Route("delete", Name = "DeleteRole")]
public async Task<IActionResult> DeleteRoleAsync(DeleteRoleDto model)
{
    var result = await _service.DeleteRoleServiceAsync(User, model);

    if (result.IsError)
        return BadRequest(result);

    return Ok(result);
}

}


