using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoreHub.API.Data;
using StoreHub.API.DTOs;
using StoreHub.API.Models;
using BCrypt.Net;

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(UserRegisterDto dto)
    {
        // Check if username or email is already taken
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Username is already taken." });

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email is already in use." });

        // Hash the password securely using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = "Customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate token and automatically log the user in after successful registration
        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto(
            token,
            user.Username,
            user.Role,
            DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:DurationInDays"]))
        );

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto dto)
    {
        // Verify user existence
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        // Verify password hash
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized(new { message = "Invalid username or password." });

        // Generate JWT token on successful login
        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto(
            token,
            user.Username,
            user.Role,
            DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:DurationInDays"]))
        );

        return Ok(response);
    }

    // Generates a secure JSON Web Token for authenticated users
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationDays = Convert.ToDouble(jwtSettings["DurationInDays"]);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
