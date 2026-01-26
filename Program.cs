using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pos_service.Data;
using pos_service.Exceptions;
using pos_service.Middlewares;
using pos_service.Repositories;
using pos_service.Repositories.Permissions;
using pos_service.Security;
using pos_service.Services;
using pos_service.Services.Common;
using pos_service.Services.Common.Cache;
using pos_service.Services.Permissions;
using System.Text;

var options = new WebApplicationOptions
{
    WebRootPath = "wwwroot" // desired path
};
var builder = WebApplication.CreateBuilder(options);
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];

// 1. Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//----Security Registration----//
// Register the Hashing Service
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
// Register the JWT Generation Service
builder.Services.AddScoped<IJwtGenerator, JwtGeneratorService>();
// Token validator service used during JWT validation   *Currently this not using REF - 000123*
//builder.Services.AddScoped<ITokenValidator, TokenValidatorService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(jwtKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddHttpContextAccessor();

// In-memory caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Add services to the container.
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<pos_service.Services.Roles.IRoleService, pos_service.Services.Roles.RoleService>();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<pos_service.Repositories.Roles.IRoleRepository, pos_service.Repositories.Roles.RoleRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<ISettingService, SettingService>();
// Backup schedule repository
builder.Services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
builder.Services.AddScoped<IBackupLocationRepository, BackupLocationRepository>();
builder.Services.AddScoped<IBackupHistoryRepository, BackupHistoryRepository>();
// BackupService dependencies are registered; BackupService registered below

// FIXED: DbContext registration with all dependencies
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// This scans your project for classes that inherit from AutoMapper.Profile
// and registers their mapping configurations.
builder.Services.AddAutoMapper(typeof(Program));

// Register backup services
builder.Services.AddScoped<pos_service.Services.Backup.IBackupService, pos_service.Services.Backup.BackupService>();
builder.Services.AddHostedService<pos_service.Services.Backup.BackupSchedulerService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS to allow requests from frontend running on localhost and network IP
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost", "http://172.20.10.5", "http://192.168.1.5")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Runtime DB Seeding ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    // Apply migrations and seed initial data
    await DbInitializer.SeedAsync(context, passwordHasher);
}

// Use global exception handler middleware
app.UseGlobalExceptionHandler();

// Enable Static File Serving 
// This middleware is essential for serving files from the designated WebRootPath.
app.UseStaticFiles();

// Enable CORS using the defined policy
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();