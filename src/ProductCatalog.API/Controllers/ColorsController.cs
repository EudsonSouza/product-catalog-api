using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColorsController : ControllerBase
{
    private readonly IColorService _colorService;

    public ColorsController(IColorService colorService)
    {
        _colorService = colorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ColorDto>), 200)]
    public async Task<ActionResult<IEnumerable<ColorDto>>> GetColors()
    {
        var colors = await _colorService.GetAllAsync();
        return Ok(colors);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<ColorDto>), 200)]
    public async Task<ActionResult<IEnumerable<ColorDto>>> GetActiveColors()
    {
        var colors = await _colorService.GetActiveAsync();
        return Ok(colors);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ColorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ColorDto>> GetColor(Guid id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null)
            return NotFound();

        return Ok(color);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ColorDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ColorDto>> CreateColor([FromBody] CreateColorDto request)
    {
        try
        {
            var color = await _colorService.CreateAsync(request);
            return CreatedAtAction(nameof(GetColor), new { id = color.Id }, color);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationProblem(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ColorDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ColorDto>> UpdateColor(Guid id, [FromBody] UpdateColorDto request)
    {
        try
        {
            var color = await _colorService.UpdateAsync(id, request);
            if (color == null)
                return NotFound();

            return Ok(color);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationProblem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteColor(Guid id)
    {
        var deleted = await _colorService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
