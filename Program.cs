using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation; // Gerekli
using FluentValidation.AspNetCore; // Gerekli

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(); // 1. FluentValidation motorunu çalıştır
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // 2. Bu projenin (Program) içindeki tüm "Validator" yazan dosyaları otomatik bul ve kullan

builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<StoreHub.API.Mappings.MappingProfile>();
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// JWT Authentication ayarları:
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});
builder.Services.AddOpenApi();
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Internet dışarıdan 'wwwroot' içindeki resimleri görebilsin diye bu izin verilir.

app.UseAuthentication(); // Önce kimlik doğrula (Kimlik kartın var mı?)
app.UseAuthorization(); // Sonra yetki kontrolü yap (Girmeye hakkın var mı?)

app.MapScalarApiReference();

app.MapControllers();

app.Run();
