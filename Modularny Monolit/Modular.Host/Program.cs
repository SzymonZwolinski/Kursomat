using Modular.Modules.Courses;
using Modular.Modules.Users;
using Modular.Modules.Sales;
using Modular.Modules.Carts;
using Modular.Shared.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modular.Modules.Carts.EventHandlers;
using Modular.Modules.Courses.EventHandlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var jwtSecret = builder.Configuration.GetSection("Jwt:Secret").Value ?? "FallbackSecretKeyForJwtAuthentication12345!@#";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddControllers();

builder.Services.AddCoursesModule(connectionString);
builder.Services.AddUsersModule(connectionString);
builder.Services.AddSalesModule(connectionString);
builder.Services.AddCartsModule(connectionString);

builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<OrderCompletedEvent>, OrderCompletedCartCleaner>();
builder.Services.AddScoped<IDomainEventHandler<OrderCompletedEvent>, OrderCompletedCourseGranter>();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
