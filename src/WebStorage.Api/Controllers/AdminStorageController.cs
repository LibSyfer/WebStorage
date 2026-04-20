using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStorage.Application.Auth;
using WebStorage.Application.StorageAccounts;

namespace WebStorage.Api.Controllers;

[ApiController]
[Route("api/admin/storage")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminStorageController(IStorageAccountService storageAccountService) : ControllerBase
{
    public sealed record ChangeQuotaRequest(long MaxBytes);

    [HttpPut("users/{userId}/quota")]
    public async Task<ActionResult<UserStorageDto>> ChangeQuota([FromRoute] string userId, [FromBody] ChangeQuotaRequest request, CancellationToken cancellationToken)
    {
        var dto = await storageAccountService.SetQuotaAsync(userId, request.MaxBytes, cancellationToken);
        return Ok(dto);
    }
}

