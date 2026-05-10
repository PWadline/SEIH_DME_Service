using Core.Application.Interface.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Security.Permission;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserPermissionService permissionService,
        IPermissionExclusionService exclusionService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        Console.WriteLine("===== PERMISSION DEBUG =====");
        Console.WriteLine($"RAW PATH: {context.Request.Path}");
        Console.WriteLine($"NORMALIZED PATH: {path}");
        Console.WriteLine($"METHOD: {method}");

        if (exclusionService.IsExcluded(method, path))
        {
            Console.WriteLine("✅ EXCLUDED PATH (bypass permission)");
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            Console.WriteLine("❌ USER NOT AUTHENTICATED");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        var userId = Guid.Parse(userIdClaim.Value);

        Console.WriteLine($"USER ID: {userId}");
        Console.WriteLine($"IS AUTHENTICATED: {context.User.Identity?.IsAuthenticated}");

        var authorized = await permissionService.HasPermissionAsync(userId, method, path);

        Console.WriteLine($"AUTHORIZED: {authorized}");

        if (!authorized)
        {
            Console.WriteLine("❌ ACCESS DENIED");
            Console.WriteLine($"PATH FAILED: {path}");
            Console.WriteLine($"METHOD FAILED: {method}");
            Console.WriteLine($"USER FAILED: {userId}");

            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden");
            return;
        }
        await _next(context);
    }
}


