using Ecommerce.Extension;
using Ecommerce.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

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
