using UserManagementApi.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IAuthService
{
  Task<object> LoginAsync(LoginRequest request);
  Task<object> RegisterAsync(RegisterRequest request);
}