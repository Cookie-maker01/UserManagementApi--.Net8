using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
  private readonly AppDbContext _db;

  public UsersController(AppDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var users =await _db.Users.ToListAsync();
    return Ok(users);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(int id)
  {
    var user = await _db.Users.FindAsync(id);
    if(user == null) return NotFound();
    return Ok(user);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, User updatedUser)
  {
    var user = await _db.Users.FindAsync(id);
    if(user == null) return NotFound();

    user.Username = updatedUser.Username;
    user.Email = updatedUser.Email;
    await _db.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var user = await _db.Users.FindAsync(id);
    if(user == null) return NotFound();

    _db.Users.Remove(user);
    await _db.SaveChangesAsync();

    return NoContent();
  }
}