using Microsoft.AspNetCore.Mvc;
using UserManagementApi.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
   private readonly IAuthService _authService;

   public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginRequest request)
  {
    var result = await _authService.LoginAsync(request);
    return Ok(result);
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
    var result = await _authService.RegisterAsync(request);
    return Ok(result);
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh(RefreshTokenRequest request)
  {
    var result = await _authService.RefreshAsync(request);
    return Ok(result);
  }
}