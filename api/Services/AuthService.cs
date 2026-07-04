using Pijeen.API.Data;
using Pijeen.API.Models;
using Pijeen.API.Models.DTOs;
using Pijeen.API.Utilities;
using BCrypt.Net;

namespace Pijeen.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        var existingUser = _context.Users.FirstOrDefault(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username already exists"
            };
        }

        // Create new user
        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserType = request.UserType
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate token
        var token = JwtTokenGenerator.GenerateToken(user, _configuration);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            User = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User account is inactive"
            };
        }

        var token = JwtTokenGenerator.GenerateToken(user, _configuration);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType
            }
        };
    }
}
