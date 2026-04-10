using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Data;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly AppDbContext _db;
  private readonly IConfiguration _configuration;

  public AuthController(AppDbContext db, IConfiguration configuration)
  {
    _db = db;
    _configuration = configuration;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var user = new User
    {
      Username = request.Username,
      Email = request.Email,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
      Role = request.Role
    };

    _db.Users.Add(user);
    await _db.SaveChangesAsync();

    return Ok(user);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginRequest request)
  {
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if(user == null) return BadRequest("User not found");

    if(!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
      return BadRequest("Invalid password");

    var claims = new[]
    {
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);
    
    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    var refreshToken = new RefreshToken
    {
      Token = Guid.NewGuid().ToString(),
      UserId = user.Id,
      Expires = DateTime.UtcNow.AddDays(7)
    };

    _db.RefreshTokens.Add(refreshToken);
    await _db.SaveChangesAsync();

    return Ok (new 
    { 
      accessToken = jwt,
      refreshToken = refreshToken.Token });
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh(RefreshTokenRequest request)
  {
    var storedToken = await _db.RefreshTokens
       .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

    if(storedToken == null || storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
    {
      return Unauthorized("Invalid refresh token");
    }

    storedToken.IsRevoked = true;
    await _db.SaveChangesAsync();

    var user = await _db.Users.FindAsync(storedToken.UserId);
    if (user == null)
       return Unauthorized("User not found");

    var claims = new[]
    {
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: creds
    );

    var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

    return Ok(new
    {
      accessToken = newAccessToken
    });
  }
}