using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductCatalog.API.Controllers;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Tests.Unit.Controllers;

public class ColorsControllerTests : IDisposable
{
    private readonly Mock<IColorService> _mockColorService;
    private readonly ColorsController _sut;

    public ColorsControllerTests()
    {
        _mockColorService = new Mock<IColorService>();
        _sut = new ColorsController(_mockColorService.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region GetColors Tests

    [Fact]
    public async Task GetColors_ReturnsOkWithColors()
    {
        // Arrange
        var colors = new List<ColorDto>
        {
            new(Guid.NewGuid(), "Red", "#FF0000", true, DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), "Blue", "#0000FF", true, DateTime.UtcNow, DateTime.UtcNow)
        };
        _mockColorService.Setup(s => s.GetAllAsync()).ReturnsAsync(colors);

        // Act
        var result = await _sut.GetColors();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedColors = Assert.IsAssignableFrom<IEnumerable<ColorDto>>(okResult.Value);
        Assert.Equal(2, returnedColors.Count());
        _mockColorService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetActiveColors Tests

    [Fact]
    public async Task GetActiveColors_ReturnsOnlyActiveColors()
    {
        // Arrange
        var activeColors = new List<ColorDto>
        {
            new(Guid.NewGuid(), "Red", "#FF0000", true, DateTime.UtcNow, DateTime.UtcNow)
        };
        _mockColorService.Setup(s => s.GetActiveAsync()).ReturnsAsync(activeColors);

        // Act
        var result = await _sut.GetActiveColors();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedColors = Assert.IsAssignableFrom<IEnumerable<ColorDto>>(okResult.Value);
        Assert.Single(returnedColors);
        Assert.True(returnedColors.First().IsActive);
        _mockColorService.Verify(s => s.GetActiveAsync(), Times.Once);
    }

    #endregion

    #region GetColor Tests

    [Fact]
    public async Task GetColor_ReturnsOkWithColor_WhenExists()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var color = new ColorDto(colorId, "Red", "#FF0000", true, DateTime.UtcNow, DateTime.UtcNow);
        _mockColorService.Setup(s => s.GetByIdAsync(colorId)).ReturnsAsync(color);

        // Act
        var result = await _sut.GetColor(colorId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedColor = Assert.IsType<ColorDto>(okResult.Value);
        Assert.Equal(colorId, returnedColor.Id);
        Assert.Equal("Red", returnedColor.Name);
        _mockColorService.Verify(s => s.GetByIdAsync(colorId), Times.Once);
    }

    [Fact]
    public async Task GetColor_ReturnsNotFound_WhenDoesNotExist()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        _mockColorService.Setup(s => s.GetByIdAsync(colorId)).ReturnsAsync((ColorDto?)null);

        // Act
        var result = await _sut.GetColor(colorId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockColorService.Verify(s => s.GetByIdAsync(colorId), Times.Once);
    }

    #endregion

    #region CreateColor Tests

    [Fact]
    public async Task CreateColor_ReturnsCreatedAtAction_WhenValid()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var request = new CreateColorDto("Red", "#FF0000", true);
        var createdColor = new ColorDto(colorId, "Red", "#FF0000", true, DateTime.UtcNow, DateTime.UtcNow);
        _mockColorService.Setup(s => s.CreateAsync(request)).ReturnsAsync(createdColor);

        // Act
        var result = await _sut.CreateColor(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedColor = Assert.IsType<ColorDto>(createdResult.Value);
        Assert.Equal("Red", returnedColor.Name);
        Assert.Equal("#FF0000", returnedColor.HexCode);
        Assert.True(returnedColor.IsActive);
        _mockColorService.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateColor_ReturnsValidationProblem_WhenNameIsEmpty()
    {
        // Arrange
        var request = new CreateColorDto("", "#FF0000");
        _mockColorService.Setup(s => s.CreateAsync(request))
            .ThrowsAsync(new ArgumentException("Name is required"));

        // Act
        var result = await _sut.CreateColor(request);

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
        _mockColorService.Verify(s => s.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateColor_ReturnsValidationProblem_WhenNameAlreadyExists()
    {
        // Arrange
        var request = new CreateColorDto("Red", "#FF0000");
        _mockColorService.Setup(s => s.CreateAsync(request))
            .ThrowsAsync(new InvalidOperationException("Color with name 'Red' already exists"));

        // Act
        var result = await _sut.CreateColor(request);

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
        _mockColorService.Verify(s => s.CreateAsync(request), Times.Once);
    }

    #endregion

    #region UpdateColor Tests

    [Fact]
    public async Task UpdateColor_ReturnsOkWithUpdatedColor_WhenValid()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var request = new UpdateColorDto("Dark Red", "#8B0000", null);
        var updatedColor = new ColorDto(colorId, "Dark Red", "#8B0000", true, DateTime.UtcNow, DateTime.UtcNow);
        _mockColorService.Setup(s => s.UpdateAsync(colorId, request)).ReturnsAsync(updatedColor);

        // Act
        var result = await _sut.UpdateColor(colorId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedColor = Assert.IsType<ColorDto>(okResult.Value);
        Assert.Equal("Dark Red", returnedColor.Name);
        Assert.Equal("#8B0000", returnedColor.HexCode);
        _mockColorService.Verify(s => s.UpdateAsync(colorId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateColor_ReturnsNotFound_WhenColorDoesNotExist()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var request = new UpdateColorDto("Red", null, null);
        _mockColorService.Setup(s => s.UpdateAsync(colorId, request)).ReturnsAsync((ColorDto?)null);

        // Act
        var result = await _sut.UpdateColor(colorId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockColorService.Verify(s => s.UpdateAsync(colorId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateColor_ReturnsValidationProblem_WhenNameAlreadyExistsForOtherColor()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var request = new UpdateColorDto("Blue", null, null);
        _mockColorService.Setup(s => s.UpdateAsync(colorId, request))
            .ThrowsAsync(new InvalidOperationException("Color with name 'Blue' already exists"));

        // Act
        var result = await _sut.UpdateColor(colorId, request);

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
        _mockColorService.Verify(s => s.UpdateAsync(colorId, request), Times.Once);
    }

    #endregion

    #region DeleteColor Tests

    [Fact]
    public async Task DeleteColor_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        _mockColorService.Setup(s => s.DeleteAsync(colorId)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteColor(colorId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockColorService.Verify(s => s.DeleteAsync(colorId), Times.Once);
    }

    [Fact]
    public async Task DeleteColor_ReturnsNotFound_WhenColorDoesNotExist()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        _mockColorService.Setup(s => s.DeleteAsync(colorId)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteColor(colorId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockColorService.Verify(s => s.DeleteAsync(colorId), Times.Once);
    }

    #endregion
}
