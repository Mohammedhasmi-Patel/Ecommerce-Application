using Ecommerce.Configurations;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.Data.Seeder.SeederClass;
using Ecommerce.Models;
using Ecommerce.Services.Address;
using Ecommerce.Services.Auth;
using Ecommerce.Services.Cart;
using Ecommerce.Services.Category;
using Ecommerce.Services.Email;
using Ecommerce.Services.Location;
using Ecommerce.Services.Order;
using Ecommerce.Services.Payment;
using Ecommerce.Services.Payment.Gateways;
using Ecommerce.Services.Product;
using Ecommerce.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Ecommerce.Extension
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AspNetUsers, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<SuperAdminSettings>(configuration.GetSection("SuperAdmin"));
            services.Configure<JwtSetting>(configuration.GetSection("JwtSetting"));
            services.Configure<StripeSettings>(configuration.GetSection("StripeConfiguration"));

            var jwtSetting = configuration.GetSection("JwtSetting").Get<JwtSetting>();

            services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSetting?.Issuer,
                    ValidAudience = jwtSetting?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting?.Key ?? string.Empty))
                };
            });

            services.AddAuthorization();

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
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
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
            });

            services.AddScoped<RoleSeeder>();
            services.AddScoped<SuperAdminSeeder>();
            services.AddScoped<CountrySeeder>();

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IPaymentGateway, RazorpayPaymentGateway>();
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            return services;
        }
    }
}
