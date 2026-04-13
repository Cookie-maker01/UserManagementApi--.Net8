using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using UserManagementApi.Repositories.Interfaces;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services.Implementations;

public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepo;
  private readonly IConfiguration _config;

  public AuthService(IUserRepository userRepo, IConfiguration config)
  {
    _userRepo = userRepo;
    _config = config;
  }

  public async Task<object> LoginAsync(LoginRequest request)
  {
    var user = await _userRepo.GetByEmailAsync(request.Email);

    if (user == null)
      throw new Exception("User not found");

    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
      throw new Exception("Invalid password");

    var claims = new[]
    {
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(15),
      signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return new
    {
      accessToken = jwt,
    };
  }

  public async Task<object> RegisterAsync(RegisterRequest request)
  {
    var user = new User
    {
      Username = request.Username,
      Email = request.Email,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
      Role = "User"
    };

    await _userRepo.AddAsync(user);

    return new
    {
      message = "User created"
    };
  }
}