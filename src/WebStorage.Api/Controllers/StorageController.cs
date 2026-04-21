using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebStorage.Application.StorageAccounts;

namespace WebStorage.Api.Controllers;

/// <summary>
/// Контроллер для получения информации о хранилище пользователя.
/// Предоставляет информацию об использованном пространстве и квоте.
/// </summary>
/// <remarks>
/// Требует аутентификацию (JWT токен в заголовке Authorization: Bearer {token})
/// </remarks>
[ApiController]
[Route("api/storage")]
[Authorize]
public sealed class StorageController(IStorageAccountService storageAccountService) : ControllerBase
{
    /// <summary>
    /// Получает информацию о хранилище текущего пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     GET /api/storage/me
    ///     Authorization: Bearer {accessToken}
    /// 
    /// Возвращает информацию об использованном пространстве,
    /// максимальной квоте.
    /// Если хранилище не создано, оно создаётся автоматически.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о хранилище пользователя (id пользователя, использовано, лимит)</returns>
    /// <response code="200">Успешно возвращена информация о хранилище</response>
    /// <response code="401">Не авторизован</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserStorageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserStorageDto>> GetMyStorage(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var dto = await storageAccountService.GetOrCreateMyAsync(userId, cancellationToken);
        return Ok(dto);
    }
}

