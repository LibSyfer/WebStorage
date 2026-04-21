using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebStorage.Application.Storage;

namespace WebStorage.Api.Controllers;

/// <summary>
/// Контроллер для управления файлами пользователя.
/// Позволяет загружать, скачивать, удалять и просматривать список файлов.
/// </summary>
/// <remarks>
/// Требует аутентификацию (JWT токен в заголовке Authorization: Bearer {token})
/// </remarks>
[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController(IFileService fileService) : ControllerBase
{
    /// <summary>
    /// Получает список всех файлов текущего пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     GET /api/files
    ///     Authorization: Bearer {accessToken}
    /// 
    /// Возвращает список с метаданными каждого файла.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список файлов с метаданными (имя, размер, дата загрузки, ID)</returns>
    /// <response code="200">Успешно возвращен список файлов</response>
    /// <response code="401">Не авторизован</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FileEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FileEntryDto>>> List(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await fileService.ListAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Загружает новый файл в хранилище пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/files/upload
    ///     Content-Type: multipart/form-data
    ///     Authorization: Bearer {accessToken}
    ///     
    ///     [binary file data]
    /// 
    /// Поддерживаются файлы любого формата и размера.
    /// Файл будет проверен относительно квоты пользователя.
    /// </remarks>
    /// <param name="file">Файл для загрузки (multipart/form-data)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>ID загруженного файла (Guid)</returns>
    /// <response code="200">Файл успешно загружен, возвращен его ID</response>
    /// <response code="400">Файл пуст или не пройдена валидация</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="413">Файл превышает лимит квоты пользователя</response>
    [HttpPost("upload")]
    [RequestSizeLimit(long.MaxValue)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<Guid>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        if (file.Length <= 0) return BadRequest("Empty file.");

        await using var stream = file.OpenReadStream();
        var id = await fileService.UploadAsync(userId, file.FileName, stream, file.Length, cancellationToken);
        return Ok(id);
    }

    /// <summary>
    /// Скачивает файл из хранилища.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     GET /api/files/{fileId}/download
    ///     Authorization: Bearer {accessToken}
    /// 
    /// Файл загружается пользователю как application/octet-stream.
    /// Возвращается только если файл принадлежит текущему пользователю.
    /// </remarks>
    /// <param name="fileId">ID файла для скачивания (Guid)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Поток файла для скачивания</returns>
    /// <response code="200">Файл успешно возвращен</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="404">Файл не найден или не принадлежит пользователю</response>
    [HttpGet("{fileId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var (stream, fileName) = await fileService.OpenReadAsync(fileId, userId, cancellationToken);
        return File(stream, "application/octet-stream", fileName);
    }

    /// <summary>
    /// Удаляет файл из хранилища пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     DELETE /api/files/{fileId}
    ///     Authorization: Bearer {accessToken}
    /// 
    /// Файл удаляется безвозвратно.
    /// Освобождаемое место вычитается из использованной квоты пользователя.
    /// </remarks>
    /// <param name="fileId">ID файла для удаления (Guid)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Никакого содержимого при успехе</returns>
    /// <response code="204">Файл успешно удален</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="404">Файл не найден или не принадлежит пользователю</response>
    [HttpDelete("{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var ok = await fileService.DeleteAsync(fileId, userId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}

