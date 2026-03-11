using FastEndpoints;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;
using Courses.Api.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

var connectionString = builder.Configuration.GetConnectionString("CoursesDb") ?? "Server=localhost,1433;Database=Microservices_Courses;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
builder.Services.AddDbContext<CoursesDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompletedCourseGranter>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMq") ?? "rabbitmq://localhost:5672", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("courses-order-completed", e =>
        {
            e.ConfigureConsumer<OrderCompletedCourseGranter>(context);
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CoursesDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseFastEndpoints();
app.Run();
