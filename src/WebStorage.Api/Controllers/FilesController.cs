using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebStorage.Application.Storage;

namespace WebStorage.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController(IFileService fileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FileEntryDto>>> List(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await fileService.ListAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult<Guid>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        if (file.Length <= 0) return BadRequest("Empty file.");

        await using var stream = file.OpenReadStream();
        var id = await fileService.UploadAsync(userId, file.FileName, stream, file.Length, cancellationToken);
        return Ok(id);
    }

    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> Download([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var (stream, fileName) = await fileService.OpenReadAsync(fileId, userId, cancellationToken);
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var ok = await fileService.DeleteAsync(fileId, userId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}

