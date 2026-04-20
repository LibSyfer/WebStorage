using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebStorage.Application.StorageAccounts;

namespace WebStorage.Api.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize]
public sealed class StorageController(IStorageAccountService storageAccountService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserStorageDto>> GetMyStorage(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var dto = await storageAccountService.GetOrCreateMyAsync(userId, cancellationToken);
        return Ok(dto);
    }
}

