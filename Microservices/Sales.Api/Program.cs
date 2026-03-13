using FastEndpoints.Security;
using FastEndpoints;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microservices.Sales.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = "VerySecretKeyForJwtAuthentication12345!@#");
builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints();

var connectionString = builder.Configuration.GetConnectionString("SalesDb") ?? "Server=localhost,1433;Database=Microservices_Sales;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost:5672", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();
app.Run();
