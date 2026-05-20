using GuardianEye.Server.Data;
using GuardianEye.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Database - Entity Framework Core with SQL Server LocalDB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=GuardianEyeDb;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<GuardianEyeDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "GuardianEyeSuperSecretKey2024!@#$%";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GuardianEye.Server";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GuardianEye.Client";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            if (context.AuthenticateFailure is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Background services
builder.Services.AddHostedService<SessionCleanupService>();

builder.Services.AddControllers();

var app = builder.Build();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GuardianEyeDbContext>();
    db.Database.EnsureCreated();

    // Seed admin user if not exists
    if (!db.Users.Any(u => u.Username == "admin"))
    {
        var adminUser = new GuardianEye.Server.Models.User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            FullName = "Administrator",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(adminUser);
        db.SaveChanges();
        app.Logger.LogInformation("Seeded admin user: admin / admin");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
