using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Repositories.Interfaces;

namespace UserManagementApi.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
  private readonly AppDbContext _context;

  public RefreshTokenRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<RefreshToken?> GetByTokenAsync(string token)
  {
    return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
  }

  public async Task AddAsync(RefreshToken refreshToken)
  {
    _context.RefreshTokens.Add(refreshToken);
    await _context.SaveChangesAsync();
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }
}

