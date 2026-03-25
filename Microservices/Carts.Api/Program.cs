using MassTransit;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;
using Microservices.Carts.Api.EventHandlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("VerySecretKeyForJwtAuthentication12345!@#")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("CartsDb") ?? "Server=localhost,1433;Database=Microservices_Carts;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
builder.Services.AddDbContext<CartsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompletedCartCleaner>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost:5672", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("carts-order-completed", e =>
        {
            e.ConfigureConsumer<OrderCompletedCartCleaner>(context);
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CartsDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
