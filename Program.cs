using Ecommerce.Configurations;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.Data.Seeder.SeederClass;
using Ecommerce.Middleware;
using Ecommerce.Models;
using Ecommerce.Services.Auth;
using Ecommerce.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AspNetUsers, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<SuperAdminSettings>(builder.Configuration.GetSection("SuperAdmin"));
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JwtSetting"));

builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<SuperAdminSeeder>();
builder.Services.AddScoped<CountrySeeder>();

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITokenService, TokenService>();


var app = builder.Build();

// Run Database Seeders on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleSeeder = services.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedAsync();

        var superAdminSeeder = services.GetRequiredService<SuperAdminSeeder>();
        await superAdminSeeder.SeedAsync();

        var countrySeeder = services.GetRequiredService<CountrySeeder>();
        await countrySeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


app.UseStaticFiles();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapGet("/", () => "Hello");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
