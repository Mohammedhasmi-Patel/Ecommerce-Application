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

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<CountrySeeder>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IEmailService,EmailService>();
builder.Services.AddTransient<IAuthService, AuthService>();


var app = builder.Build();
app.UseStaticFiles();
app.UseMiddleware<GlobalExceptionMiddleware>();


app.MapGet("/",() => "Hello");

app.UseAuthorization();

app.MapControllers();

app.Run();
