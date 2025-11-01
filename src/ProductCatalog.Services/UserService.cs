using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Data;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Services.DTOs;
using ProductCatalog.Services.Interfaces;

namespace ProductCatalog.Services;

public class UserService : IUserService
{
    private const string PostgresUniqueViolationCode = "23505";
    private const string DuplicateKeyErrorMessage = "duplicate key";
    private const string AdminEmailsConfigSection = "AdminEmails";
    private const string UserCreationFailedMessage = "User creation failed and retry lookup returned null";

    private readonly ProductCatalogDbContext _context;
    private readonly HashSet<string> _adminEmails;

    public UserService(ProductCatalogDbContext context, IConfiguration configuration)
    {
        _context = context;
        _adminEmails = LoadAdminEmailsFromConfiguration(configuration);
    }

    public async Task<User> FindOrCreateUserAsync(GoogleUserInfo googleUserInfo)
    {
        var existingUser = await FindUserByEmailAsync(googleUserInfo.Email);

        return existingUser == null
            ? await CreateNewUserAsync(googleUserInfo)
            : await UpdateExistingUserIfNeededAsync(existingUser, googleUserInfo);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public bool IsAdminEmail(string email)
    {
        return _adminEmails.Contains(email);
    }

    private static HashSet<string> LoadAdminEmailsFromConfiguration(IConfiguration configuration)
    {
        var adminEmailsList = configuration
            .GetSection(AdminEmailsConfigSection)
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!);

        return new HashSet<string>(adminEmailsList, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private async Task<User> CreateNewUserAsync(GoogleUserInfo googleUserInfo)
    {
        var newUser = BuildUserFromGoogleInfo(googleUserInfo);
        _context.Users.Add(newUser);

        try
        {
            await _context.SaveChangesAsync();
            return newUser;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return await HandleRaceConditionAsync(googleUserInfo.Email);
        }
    }

    private User BuildUserFromGoogleInfo(GoogleUserInfo googleUserInfo)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = googleUserInfo.Email,
            Name = googleUserInfo.Name,
            PictureUrl = googleUserInfo.PictureUrl,
            IsAdmin = IsAdminEmail(googleUserInfo.Email),
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<User> HandleRaceConditionAsync(string email)
    {
        var user = await FindUserByEmailAsync(email);
        return user ?? throw new InvalidOperationException(UserCreationFailedMessage);
    }

    private async Task<User> UpdateExistingUserIfNeededAsync(User user, GoogleUserInfo googleUserInfo)
    {
        if (!HasUserInfoChanged(user, googleUserInfo))
        {
            return user;
        }

        UpdateUserInfoFromGoogle(user, googleUserInfo);
        await _context.SaveChangesAsync();
        return user;
    }

    private static bool HasUserInfoChanged(User user, GoogleUserInfo googleUserInfo)
    {
        return user.Name != googleUserInfo.Name || user.PictureUrl != googleUserInfo.PictureUrl;
    }

    private static void UpdateUserInfoFromGoogle(User user, GoogleUserInfo googleUserInfo)
    {
        user.Name = googleUserInfo.Name;
        user.PictureUrl = googleUserInfo.PictureUrl;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var errorMessage = ex.InnerException?.Message ?? string.Empty;
        return errorMessage.Contains(PostgresUniqueViolationCode) ||
               errorMessage.Contains(DuplicateKeyErrorMessage);
    }
}
