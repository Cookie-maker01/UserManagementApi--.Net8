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
  private readonly IRefreshTokenRepository _refreshRepo;
  private readonly IUserRepository _userRepo;
  private readonly IConfiguration _config;

  public AuthService(IUserRepository userRepo, IRefreshTokenRepository refreshRepo, IConfiguration config)
  {
    _userRepo = userRepo;
    _refreshRepo = refreshRepo;
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

    var refreshToken = new RefreshToken
    {
      Token = Guid.NewGuid().ToString(),
      UserId = user.Id,
      Expires = DateTime.UtcNow.AddDays(7)
    };

    await _refreshRepo.AddAsync(refreshToken);

    return new
    {
      accessToken = jwt,
      refreshToken = refreshToken.Token
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

  public async Task<object> RefreshAsync(RefreshTokenRequest request)
  {
     var storedToken = await _refreshRepo.GetByTokenAsync(request.RefreshToken);

     if (storedToken == null || storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
        throw new Exception("Invalid refresh token");

      storedToken.IsRevoked = true;
      await _refreshRepo.SaveChangesAsync();

     var user = await _userRepo.GetByIdAsync(storedToken.UserId);

     if (user == null)
        throw new Exception("User not found");

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

     var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

     return new
     {
        accessToken = newAccessToken
     };
  }
}