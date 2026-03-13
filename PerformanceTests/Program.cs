using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace PerformanceTests;

class Program
{
    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var targetName = config["Target"];
        var baseUrl = config[$"Targets:{targetName}"];

        if (string.IsNullOrEmpty(baseUrl))
        {
            Console.WriteLine("Invalid target configuration");
            return;
        }

        Console.WriteLine($"Starting Performance Tests against {targetName} at {baseUrl}");

        var httpClient = new HttpClient();

        var scenario = Scenario.Create("order_flow", async context =>
        {
            var userId = Guid.NewGuid().ToString();
            var email = $"user_{userId}@test.com";
            var password = "Password123!";

            // 1. Register
            var registerPayload = new
            {
                Login = email,
                Email = email,
                Password = password,
                FullName = "Test User"
            };

            var registerContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
            var registerRequest = Http.CreateRequest("POST", $"{baseUrl}/api/account/create")
                .WithHeader("Content-Type", "application/json")
                .WithBody(registerContent);

            var msRegisterRequest = Http.CreateRequest("POST", $"{baseUrl}/api/users/register")
                .WithHeader("Content-Type", "application/json")
                .WithBody(registerContent);

            var reqToUse = targetName == "Microservices" || targetName == "ModularMonolith" ? msRegisterRequest : registerRequest;
            var registerResponse = await Http.Send(httpClient, reqToUse);

            if (registerResponse.IsError) return registerResponse;

            // 2. Login
            var loginPayload = new
            {
                Login = email,
                Password = password
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
            var loginRequest = Http.CreateRequest("POST", targetName == "Microservices" || targetName == "ModularMonolith" ? $"{baseUrl}/api/users/login" : $"{baseUrl}/api/account/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(loginContent);

            var loginResponse = await Http.Send(httpClient, loginRequest);
            if (loginResponse.IsError) return loginResponse;

            var loginResultString = await loginResponse.Payload.Value.Content.ReadAsStringAsync();
            using var loginResultDoc = JsonDocument.Parse(loginResultString);
            var token = loginResultDoc.RootElement.GetProperty("token").GetString();

            // 3. Create Course
            var createCoursePayload = new
            {
                Name = "Test Course",
                Title = "Test Course",
                Description = "A great course",
                Price = 99.99m
            };

            var createCourseContent = new StringContent(JsonSerializer.Serialize(createCoursePayload), Encoding.UTF8, "application/json");
            var createCourseRequest = Http.CreateRequest("POST", $"{baseUrl}/api/courses")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(createCourseContent);

            var createCourseResponse = await Http.Send(httpClient, createCourseRequest);
            if (createCourseResponse.IsError) return createCourseResponse;

            var courseResultString = await createCourseResponse.Payload.Value.Content.ReadAsStringAsync();
            using var courseResultDoc = JsonDocument.Parse(courseResultString);
            var courseId = courseResultDoc.RootElement.GetProperty("id").GetString();

            // 4. Create Order
            var createOrderPayload = new
            {
                UserId = userId,
                CourseIds = new[] { courseId },
                TotalPrice = 99.99m
            };

            var createOrderContent = new StringContent(JsonSerializer.Serialize(createOrderPayload), Encoding.UTF8, "application/json");
            var createOrderRequest = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(createOrderContent);

            var createOrderResponse = await Http.Send(httpClient, createOrderRequest);

            return createOrderResponse;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFileName($"performance_report_{targetName}")
            .WithReportFolder("./reports")
            .WithReportFormats(ReportFormat.Csv, ReportFormat.Html, ReportFormat.Md)
            .Run();
    }
}
