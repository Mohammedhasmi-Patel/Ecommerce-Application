using Ecommerce.Data;
using Ecommerce.Data.Seeder.SeederClass;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<CountrySeeder>();


var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider
//        .GetRequiredService<CountrySeeder>();

//    await seeder.SeedAsync();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
