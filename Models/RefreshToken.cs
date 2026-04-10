namespace UserManagementApi.Models;

public class RefreshToken
{
  public int Id { get; set; }

  public string Token { get; set; } = "";

  public int UserId { get; set; }

  public User User { get; set; } = null!;

  public DateTime Expires { get; set; }

  public bool IsRevoked { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}