using Ecommerce.Configurations;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.Data.Seeder.SeederClass;
using Ecommerce.Middleware;
using Ecommerce.Models;
using Ecommerce.Services.Auth;
using Ecommerce.Services.Category;
using Ecommerce.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Ecommerce.Services.Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AspNetUsers, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<SuperAdminSettings>(builder.Configuration.GetSection("SuperAdmin"));
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JwtSetting"));

JwtSetting jwtSetting = builder.Configuration.GetSection("JwtSetting").Get<JwtSetting>();

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
            ValidIssuer = jwtSetting.Issuer,
            ValidAudience = jwtSetting.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token only (without Bearer prefix)"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
            []
        });
});
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<SuperAdminSeeder>();
builder.Services.AddScoped<CountrySeeder>();

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Run Database Seeders on startup
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var logger = services.GetRequiredService<ILogger<Program>>();
//     try
//     {
//         var roleSeeder = services.GetRequiredService<RoleSeeder>();
//         await roleSeeder.SeedAsync();

//         var superAdminSeeder = services.GetRequiredService<SuperAdminSeeder>();
//         await superAdminSeeder.SeedAsync();

//         var countrySeeder = services.GetRequiredService<CountrySeeder>();
//         await countrySeeder.SeedAsync();
//     }
//     catch (Exception ex)
//     {
//         logger.LogError(ex, "An error occurred while seeding the database.");
//     }
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapGet("/", () => "Hello");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
