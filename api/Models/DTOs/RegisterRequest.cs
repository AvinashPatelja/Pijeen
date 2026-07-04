namespace Pijeen.API.Models.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string UserType { get; set; } = "Farmer"; // Default to Farmer
}
