using Ecommerce.Data;
using Ecommerce.Data.Seeder.SeederClass;
using Ecommerce.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<CountrySeeder>();

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();


//app.UseHttpsRedirection();

app.MapGet("/",() => "Hello");

app.UseAuthorization();

app.MapControllers();

app.Run();
