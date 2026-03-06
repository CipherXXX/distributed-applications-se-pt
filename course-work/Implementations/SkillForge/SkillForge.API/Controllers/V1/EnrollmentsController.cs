using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;

namespace SkillForge.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;
    private readonly IValidator<CreateEnrollmentDto> _createValidator;
    private readonly IValidator<UpdateEnrollmentDto> _updateValidator;

    public EnrollmentsController(IEnrollmentService service, IValidator<CreateEnrollmentDto> createValidator, IValidator<UpdateEnrollmentDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private int? GetCurrentUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(id, out var userId) ? userId : null;
    }

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    [ProducesResponseType(typeof(Application.Common.PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? studentId = null, [FromQuery] int? courseId = null, [FromQuery] bool? completed = null, [FromQuery] string? sortBy = null, [FromQuery] bool sortDesc = false, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPagedAsync(page, pageSize, studentId, courseId, completed, sortBy, sortDesc, cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(Application.Common.PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] bool sortDesc = false, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();
        var result = await _service.GetEnrollmentsForUserAsync(userId.Value, page, pageSize, sortBy, sortDesc, cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(Application.Common.PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] int? studentId = null, [FromQuery] int? courseId = null, [FromQuery] bool? completed = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPagedAsync(page, pageSize, studentId, courseId, completed, null, false, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        EnrollmentDto? item;
        if (IsAdmin)
            item = await _service.GetByIdAsync(id, cancellationToken);
        else
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            item = await _service.GetByIdForUserAsync(id, userId.Value, cancellationToken);
        }
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("me")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnrollMe([FromBody] EnrollMeRequest request, CancellationToken cancellationToken)
    {
        if (IsAdmin)
            return BadRequest(new { error = "Use the standard create endpoint for admin." });
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();
        var created = await _service.EnrollUserInCourseAsync(userId.Value, request.CourseId, cancellationToken);
        if (created == null)
            return BadRequest(new { error = "Already enrolled in this course or invalid course." });
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto dto, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentDto dto, CancellationToken cancellationToken)
    {
        if (!IsAdmin)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            if (await _service.GetByIdForUserAsync(id, userId.Value, cancellationToken) == null)
                return NotFound();
        }
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!IsAdmin)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            if (await _service.GetByIdForUserAsync(id, userId.Value, cancellationToken) == null)
                return NotFound();
        }
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
