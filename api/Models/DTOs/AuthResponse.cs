namespace Pijeen.API.Models.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string UserType { get; set; } = null!;
}
