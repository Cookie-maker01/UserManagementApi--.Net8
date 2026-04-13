using UserManagementApi.Models;

namespace UserManagementApi.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
  Task<RefreshToken?> GetByTokenAsync(string token);

  Task AddAsync(RefreshToken refreshToken);

  Task SaveChangesAsync();
}

