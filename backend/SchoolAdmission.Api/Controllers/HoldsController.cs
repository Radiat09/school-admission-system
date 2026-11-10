using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController : ControllerBase
{
    private readonly IAdmissionService _admissionService;

    public HoldsController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    [HttpGet("{holdId}")]
    public async Task<IActionResult> GetHold(string holdId, CancellationToken cancellationToken)
    {
        var dto = await _admissionService.GetHoldAsync(holdId, cancellationToken);
        return Ok(dto);
    }

    [HttpPost("{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId, CancellationToken cancellationToken)
    {
        var studentId = Request.Headers["X-Student-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new { error = "Bad request", message = "X-Student-Id header is required." });
        }

        var response = await _admissionService.ConfirmAsync(holdId, studentId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, CancellationToken cancellationToken)
    {
        var studentId = Request.Headers["X-Student-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new { error = "Bad request", message = "X-Student-Id header is required." });
        }

        await _admissionService.CancelAsync(holdId, studentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("/me/holds")]
    public async Task<IActionResult> GetMyHolds(CancellationToken cancellationToken)
    {
        var studentId = Request.Headers["X-Student-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new { error = "Bad request", message = "X-Student-Id header is required." });
        }

        var holds = await _admissionService.GetMyHoldsAsync(studentId, cancellationToken);
        return Ok(holds);
    }
}
