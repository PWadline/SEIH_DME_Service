using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace WebAPI.Controllers.SEIH.HOSPITALMANAGER;

[Route("seih/network-hospital")]
[ApiController]
[Authorize] // si tu veux protéger
public class NetworkHospitalController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public NetworkHospitalController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetHospitals()
    {
        var currentHospitalId = _config.GetValue<Guid>("SEIH:HospitalId");

        var hospitals = await _context.Hospitals
            .Where(h => h.IsActive && h.Id != currentHospitalId)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Department
            })
            .ToListAsync();

        return Ok(hospitals);
    }
}