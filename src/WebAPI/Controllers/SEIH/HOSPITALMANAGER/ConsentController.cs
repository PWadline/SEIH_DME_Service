using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("seih/consent")]
[ApiController]
[AllowAnonymous]
public class ConsentController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConsentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetConsents()
    {
        var consents = await _context.Consents
            .Where(c => c.IsGranted)
            .Select(c => new
            {
                c.Id,
                // c.Label
            })
            .ToListAsync();

        return Ok(consents);
    }
}