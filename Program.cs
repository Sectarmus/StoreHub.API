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

// CORS Politikası: Frontend (React/Vue/Flutter vs.) uygulamanın bu API'ye erişebilmesi için güvenlik izni.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Herhangi bir domain'den (site) istek gelebilir.
              .AllowAnyMethod()   // Her türlü HTTP metoduna (GET, POST, DELETE, PUT) izin ver.
              .AllowAnyHeader();  // Her türlü Header'a (örneğin Authorization) izin ver.
    });
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

app.UseCors("AllowAll"); // React / Yabancı Cihazlar için "Sınır Kapılarını Aç" kuralını devreye sok.

app.UseAuthentication(); // Önce kimlik doğrula (Kimlik kartın var mı?)
app.UseAuthorization(); // Sonra yetki kontrolü yap (Girmeye hakkın var mı?)

app.MapScalarApiReference();

app.MapControllers();

// UYGULAMA ÇALIŞMADAN HEMEN ÖNCE: Veritabanı Tohumlama (Seeding) Adımı
// Kendi sanal "alanımızı / evrenimizi (scope)" yaratıyoruz ki servislere ulaşabilelim
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Program içerisindeki DbContext dosyasını ödünç alıyoruz
        var context = services.GetRequiredService<AppDbContext>();

        // Eğer veritabanı yepyeni bir sunucuya atılırsa eksik Database kurulumlarını (Migration) otomatik uygular!
        // Bu, sunucu (Docker vb.) dağıtımlarında hayat kurtaran profesyonel bir kod parçasıdır.
        await context.Database.MigrateAsync();

        // Tohumlama sınıfımızı çağırıp veritabanına can veriyoruz:
        await StoreHub.API.Data.AppDbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        // Gelişmiş projelerde buraya bir "Logger" yerleştirilir, şimdilik görmezden gelinebilir
        Console.WriteLine("Veritabanı oluşturulurken bir hata oluştu: " + ex.Message);
    }
}

app.Run();
