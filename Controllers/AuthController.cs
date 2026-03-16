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
        _configuration = configuration; // appsettings.json dosyasını okumak için
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(UserRegisterDto dto)
    {
        // 1. Kullanıcı adı veya e-posta kullanımda mı?
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Bu kullanıcı adı zaten alınmış." });

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanımda." });

        // 2. Şifreyi Hash'le (BCrypt kullanarak geriye döndürülemez hale getiriyoruz)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = "Customer" // Varsayılan rol
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 3. Başarılı kayıt sonrası direkt token üretip giriş yapmış sayabiliriz
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
        // 1. Kullanıcıyı bul
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." }); // Bilerek spesifik hata vermiyoruz ki hackerlar anlamasın

        // 2. Şifre doğru mu kontrol et
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });

        // 3. Şifre doğruysa Token üret
        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto(
            token,
            user.Username,
            user.Role,
            DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:DurationInDays"]))
        );

        return Ok(response);
    }

    // --- JWT Üretim Motoru (Mühendislik Harikası) ---
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"];

        // 1. Kimlik Kartının İçine Yazılacak Bilgiler (Claims)
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token'a özel benzersiz ID
        };

        // 2. İmza (Şifreleme)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Bu şifreleme algoritmasıdır

        // 3. Kartın Son Kullanma Tarihi
        var expirationDays = Convert.ToDouble(jwtSettings["DurationInDays"]);

        // 4. Kartı (Token) Oluştur
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: creds
        );

        // Şifreli metin olarak geri dön
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
