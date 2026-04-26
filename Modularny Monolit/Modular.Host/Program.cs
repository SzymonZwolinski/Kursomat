using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Modular.Modules.Carts;
using Modular.Modules.Carts.Data;
using Modular.Modules.Carts.EventHandlers;
using Modular.Modules.Courses;
using Modular.Modules.Courses.Data;
using Modular.Modules.Courses.EventHandlers;
using Modular.Modules.Sales;
using Modular.Modules.Sales.Data;
using Modular.Modules.Users;
using Modular.Modules.Users.Data;
using Modular.Shared.Events;
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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Modular.Modules.Courses.Data.CoursesDbContext>();
    context.Database.Migrate();

    var salesContext = scope.ServiceProvider.GetRequiredService<Modular.Modules.Sales.Data.SalesDbContext>();
    salesContext.Database.Migrate();

    var usersContext = scope.ServiceProvider.GetRequiredService<Modular.Modules.Users.Data.UsersDbContext>();
    usersContext.Database.Migrate();

    var cartsContext = scope.ServiceProvider.GetRequiredService<Modular.Modules.Carts.Data.CartsDbContext>();
    cartsContext.Database.Migrate();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();