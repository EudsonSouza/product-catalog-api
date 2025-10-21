using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ProductCatalogDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await Context.Users
            .AnyAsync(u => u.Username == normalizedUsername);
    }
}
