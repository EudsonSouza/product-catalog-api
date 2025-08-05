using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;

namespace ProductCatalog.Tests.Unit.Data;

public class UnitOfWorkTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductCatalogDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }


    [Fact]
    public void Products_Should_Return_Same_Instance_On_Multiple_Calls()
    {
        var products1 = _unitOfWork.Products;
        var products2 = _unitOfWork.Products;

        products1.Should().BeSameAs(products2);
        products1.Should().NotBeNull();
    }

    [Fact]
    public void Categories_Should_Return_Same_Instance_On_Multiple_Calls()
    {
        var categories1 = _unitOfWork.Categories;
        var categories2 = _unitOfWork.Categories;

        categories1.Should().BeSameAs(categories2);
        categories1.Should().NotBeNull();
    }

    [Fact]
    public void Colors_Should_Return_Same_Instance_On_Multiple_Calls()
    {
        var colors1 = _unitOfWork.Colors;
        var colors2 = _unitOfWork.Colors;

        colors1.Should().BeSameAs(colors2);
        colors2.Should().NotBeNull();
    }

    [Fact]
    public void Sizes_Should_Return_Same_Instance_On_Multiple_Calls()
    {
        var sizes1 = _unitOfWork.Sizes;
        var sizes2 = _unitOfWork.Sizes;

        sizes1.Should().BeSameAs(sizes2);
        sizes1.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Return_Number_Of_Affected_Entities()
    {
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitTransactionAsync_Should_Throw_When_No_Transaction_In_Progress()
    {
        var act = async () => await _unitOfWork.CommitTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No transaction is in progress.");
    }

    [Fact]
    public async Task RollbackTransactionAsync_Should_Throw_When_No_Transaction_In_Progress()
    {
        var act = async () => await _unitOfWork.RollbackTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No transaction is in progress.");
    }

    [Fact]
    public void Dispose_Should_Not_Throw()
    {
        var act = () => _unitOfWork.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Multiple_Dispose_Calls_Should_Not_Throw()
    {
        _unitOfWork.Dispose();
        var act = () => _unitOfWork.Dispose();
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
