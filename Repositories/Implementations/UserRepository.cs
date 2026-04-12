using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Repositories.Interfaces;

namespace UserManagementApi.Repositories.Implementations;

public class UserRepository : IUserRepository
{
  private readonly AppDbContext _context;

  public UserRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
  }

  public async Task<User?> GetByIdAsync(int id)
  {
    return await _context.Users.FindAsync(id);
  }

  public async Task AddAsync(User user)
  {
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
  }
}