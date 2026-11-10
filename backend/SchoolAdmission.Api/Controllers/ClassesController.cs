using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly IAdmissionService _admissionService;

    public ClassesController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Bad request", message = "Invalid class data." });
        }

        var dto = await _admissionService.CreateClassAsync(request.Name, request.TotalSeats, cancellationToken);
        return CreatedAtAction(nameof(GetClass), new { classId = dto.Id }, dto);
    }

    [HttpGet]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        var dtos = await _admissionService.GetClassesAsync(cancellationToken);
        return Ok(dtos);
    }

    [HttpGet("{classId}")]
    public async Task<IActionResult> GetClass(string classId, CancellationToken cancellationToken)
    {
        var dto = await _admissionService.GetClassAsync(classId, cancellationToken);
        if (dto == null)
        {
            return NotFound(new { error = "Not found", message = $"Class with ID '{classId}' was not found." });
        }
        return Ok(dto);
    }

    [HttpPost("{classId}/holds")]
    public async Task<IActionResult> Apply(string classId, CancellationToken cancellationToken)
    {
        var studentId = Request.Headers["X-Student-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new { error = "Bad request", message = "X-Student-Id header is required." });
        }

        try
        {
            var response = await _admissionService.ApplyAsync(classId, studentId, cancellationToken);
            return Created("", response);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
