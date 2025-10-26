using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pos_service.Data;
using pos_service.Repositories;
using pos_service.Security;
using pos_service.Services;
using pos_service.Services.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var options = new WebApplicationOptions
{
    WebRootPath = "wwwroot" // desired path
};
var builder = WebApplication.CreateBuilder(options);
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];

// 1. Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddScoped<IItemRepository    , ItemRepository>();
builder.Services.AddScoped<IItemService       , ItemService>();
builder.Services.AddScoped<IContactRepository , ContactRepository>();
builder.Services.AddScoped<IContactService    , ContactService>();
builder.Services.AddScoped<ISupplierService   , SupplierService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository    , UserRepository>();
builder.Services.AddScoped<IUserService       , UserService>();
builder.Services.AddScoped<IFileStorageService,LocalFileStorageService>();

//----Security Registration----//
// Register the Hashing Service
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
// Register the JWT Generation Service
builder.Services.AddScoped<IJwtGenerator, JwtGeneratorService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(jwtKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = false,
            ValidateAudience         = false,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(key)
        };
    });

// 2. Add your AppDbContext to the services container (Dependency Injection)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// This scans your project for classes that inherit from AutoMapper.Profile
// and registers their mapping configurations.
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable Static File Serving 
// This middleware is essential for serving files from the designated WebRootPath.
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
