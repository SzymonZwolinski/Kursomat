using FastEndpoints;
using Modular.Modules.Courses;
using Modular.Modules.Users;
using Modular.Modules.Sales;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddFastEndpoints();

builder.Services.AddCoursesModule(connectionString);
builder.Services.AddUsersModule(connectionString);
builder.Services.AddSalesModule(connectionString);

var app = builder.Build();

app.UseFastEndpoints();
app.Run();