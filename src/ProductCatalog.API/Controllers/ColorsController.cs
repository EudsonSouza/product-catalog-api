using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColorsController : ControllerBase
{
    private readonly IColorRepository _colorRepository;

    public ColorsController(IColorRepository colorRepository)
    {
        _colorRepository = colorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Color>>> GetColors()
    {
        var colors = await _colorRepository.GetAllAsync();
        return Ok(colors);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Color>>> GetActiveColors()
    {
        var colors = await _colorRepository.GetActiveAsync();
        return Ok(colors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Color>> GetColor(Guid id)
    {
        var color = await _colorRepository.GetByIdAsync(id);
        if (color == null)
            return NotFound();
        
        return Ok(color);
    }
}