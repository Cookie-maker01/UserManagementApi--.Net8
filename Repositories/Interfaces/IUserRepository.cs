using UserManagementApi.Models;

namespace UserManagementApi.Repositories.Interfaces;

public interface IUserRepository
{
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByIdAsync(int id);
  Task AddAsync(User user);
}