using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStorage.Application.Files;

namespace WebStorage.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController(IFileService fileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FileEntryDto>>> List([FromQuery] string userId, CancellationToken cancellationToken)
    {
        var result = await fileService.ListAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult<Guid>> Upload([FromQuery] string userId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0) return BadRequest("Empty file.");

        await using var stream = file.OpenReadStream();
        var id = await fileService.UploadAsync(userId, file.FileName, stream, file.Length, cancellationToken);
        return Ok(id);
    }

    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> Download([FromRoute] Guid fileId, [FromQuery] string userId, CancellationToken cancellationToken)
    {
        var (stream, fileName) = await fileService.OpenReadAsync(fileId, userId, cancellationToken);
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid fileId, [FromQuery] string userId, CancellationToken cancellationToken)
    {
        var ok = await fileService.DeleteAsync(fileId, userId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}

