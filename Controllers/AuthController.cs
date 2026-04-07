using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Data;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly AppDbContext _db;

  public AuthController(AppDbContext db)
  {
    _db = db;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var user = new User
    {
      Username = request.Username,
      Email = request.Email,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
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

      return Ok("Login successful");
  }
}