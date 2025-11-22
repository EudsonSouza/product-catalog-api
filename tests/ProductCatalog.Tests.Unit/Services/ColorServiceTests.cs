using Moq;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Services;
using ProductCatalog.Services.DTOs;

namespace ProductCatalog.Tests.Unit.Services;

public class ColorServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IColorRepository> _mockColorRepo;
    private readonly ColorService _sut;

    public ColorServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockColorRepo = new Mock<IColorRepository>();
        _mockUnitOfWork.Setup(u => u.Colors).Returns(_mockColorRepo.Object);
        _sut = new ColorService(_mockUnitOfWork.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsColorDto_WhenColorExists()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var color = new Color
        {
            Id = colorId,
            Name = "Red",
            HexCode = "#FF0000",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockColorRepo.Setup(r => r.GetByIdAsync(colorId)).ReturnsAsync(color);

        // Act
        var result = await _sut.GetByIdAsync(colorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(colorId, result.Id);
        Assert.Equal("Red", result.Name);
        Assert.Equal("#FF0000", result.HexCode);
        _mockColorRepo.Verify(r => r.GetByIdAsync(colorId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllColors()
    {
        // Arrange
        var colors = new List<Color>
        {
            new() { Id = Guid.NewGuid(), Name = "Red", HexCode = "#FF0000", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Blue", HexCode = "#0000FF", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _mockColorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(colors);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _mockColorRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ReturnsColorDto_WhenValid()
    {
        // Arrange
        var dto = new CreateColorDto("Red", "#FF0000", true);
        _mockColorRepo.Setup(r => r.GetByNameAsync("Red")).ReturnsAsync((Color?)null);
        _mockColorRepo.Setup(r => r.AddAsync(It.IsAny<Color>())).ReturnsAsync((Color c) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Red", result.Name);
        Assert.Equal("#FF0000", result.HexCode);
        Assert.True(result.IsActive);
        _mockColorRepo.Verify(r => r.AddAsync(It.IsAny<Color>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateColorDto("", "#FF0000");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
        _mockColorRepo.Verify(r => r.AddAsync(It.IsAny<Color>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenNameExists()
    {
        // Arrange
        var dto = new CreateColorDto("Red", "#FF0000");
        var existingColor = new Color { Id = Guid.NewGuid(), Name = "Red" };
        _mockColorRepo.Setup(r => r.GetByNameAsync("Red")).ReturnsAsync(existingColor);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        _mockColorRepo.Verify(r => r.AddAsync(It.IsAny<Color>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedColorDto_WhenValid()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var existingColor = new Color
        {
            Id = colorId,
            Name = "Red",
            HexCode = "#FF0000",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var dto = new UpdateColorDto("Dark Red", "#8B0000", null);
        _mockColorRepo.Setup(r => r.GetByIdAsync(colorId)).ReturnsAsync(existingColor);
        _mockColorRepo.Setup(r => r.GetByNameAsync("Dark Red")).ReturnsAsync((Color?)null);
        _mockColorRepo.Setup(r => r.UpdateAsync(It.IsAny<Color>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(colorId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Dark Red", result.Name);
        Assert.Equal("#8B0000", result.HexCode);
        _mockColorRepo.Verify(r => r.UpdateAsync(It.IsAny<Color>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenColorExists()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        var color = new Color { Id = colorId, Name = "Red" };
        _mockColorRepo.Setup(r => r.GetByIdAsync(colorId)).ReturnsAsync(color);
        _mockColorRepo.Setup(r => r.DeleteAsync(colorId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(colorId);

        // Assert
        Assert.True(result);
        _mockColorRepo.Verify(r => r.DeleteAsync(colorId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenColorDoesNotExist()
    {
        // Arrange
        var colorId = Guid.NewGuid();
        _mockColorRepo.Setup(r => r.GetByIdAsync(colorId)).ReturnsAsync((Color?)null);

        // Act
        var result = await _sut.DeleteAsync(colorId);

        // Assert
        Assert.False(result);
        _mockColorRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
