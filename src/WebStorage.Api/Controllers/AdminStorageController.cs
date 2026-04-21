using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStorage.Application.Auth;
using WebStorage.Application.StorageAccounts;

namespace WebStorage.Api.Controllers;

/// <summary>
/// Контроллер администратора для управления хранилищами пользователей.
/// Позволяет администратору устанавливать квоты для пользователей.
/// </summary>
/// <remarks>
/// Требует роль Administrator.
/// Требует аутентификацию (JWT токен в заголовке Authorization: Bearer {token})
/// </remarks>
[ApiController]
[Route("api/admin/storage")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminStorageController(IStorageAccountService storageAccountService) : ControllerBase
{
    /// <summary>
    /// DTO для изменения квоты хранилища пользователя.
    /// </summary>
    /// <param name="MaxBytes">Максимальный размер хранилища в байтах</param>
    public sealed record ChangeQuotaRequest(long MaxBytes);

    /// <summary>
    /// Устанавливает квоту (лимит места) для хранилища пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     PUT /api/admin/storage/users/{userId}/quota
    ///     Authorization: Bearer {adminToken}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///         "maxBytes": 1073741824
    ///     }
    /// 
    /// MaxBytes - максимальный размер в байтах (например, 1073741824 = 1 GB).
    /// Квота может быть меньше текущего использованного места, тогда загрузка новых файлов не будет происходить пока место не освободится.
    /// </remarks>
    /// <param name="userId">ID пользователя (string)</param>
    /// <param name="request">Новое значение максимального размера хранилища в байтах</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Обновлённая информация о хранилище пользователя</returns>
    /// <response code="200">Квота успешно установлена, возвращена обновлённая информация</response>
    /// <response code="400">Некорректное значение квоты</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Пользователь не найден</response>
    [HttpPut("users/{userId}/quota")]
    [ProducesResponseType(typeof(UserStorageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStorageDto>> ChangeQuota([FromRoute] string userId, [FromBody] ChangeQuotaRequest request, CancellationToken cancellationToken)
    {
        var dto = await storageAccountService.SetQuotaAsync(userId, request.MaxBytes, cancellationToken);
        return Ok(dto);
    }
}

