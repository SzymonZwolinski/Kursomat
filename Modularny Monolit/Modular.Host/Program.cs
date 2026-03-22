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

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
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
app.MapControllers();
app.Run();
