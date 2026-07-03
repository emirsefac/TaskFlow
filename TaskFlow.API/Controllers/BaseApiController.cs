using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    // JWT içindeki "sub" claim'inden giriş yapan kullanıcının Id'sini okur
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? throw new UnauthorizedAccessException());
}
