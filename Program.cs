using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pos_service.Data;
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
        // Validate that the token's user still exists and is active on each request using TokenValidatorService

        //  * This committed because current user not cached not nee check this. REF - 000123 *

        //options.Events = new JwtBearerEvents
        //{
        //    OnTokenValidated = async context =>
        //    {
        //        var validator = context.HttpContext.RequestServices.GetService<pos_service.Services.Authentication.ITokenValidator>();
        //        if (validator == null)
        //        {
        //            context.Fail("Token validator unavailable");
        //            return;
        //        }

        //        try
        //        {
        //            await validator.ValidateTokenPrincipalAsync(context.Principal!);
        //        }
        //        catch (Exception ex)
        //        {
        //            context.Fail(ex.Message ?? "Token validation failed");
        //        }
        //    }
        //};
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

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// FIXED: DbContext registration with all dependencies
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

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

// --- Runtime DB Seeding ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    // Apply migrations and seed initial data
    await DbInitializer.SeedAsync(context, passwordHasher);
}

// Enable Static File Serving 
// This middleware is essential for serving files from the designated WebRootPath.
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();