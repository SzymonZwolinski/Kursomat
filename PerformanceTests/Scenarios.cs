using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace PerformanceTests;

public static class Scenarios
{
    // P-01: Narzut komunikacji i serializacji (Load Test - GetAllCoursesEndpoint)
    public static ScenarioProps GetP01Scenario(HttpClient httpClient, string baseUrl)
    {
        return Scenario.Create("p01_get_all_courses", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/courses");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-02: Koszt rozproszonych transakcji (Stress Test - CreateOrderEndpoint)
    public static ScenarioProps GetP02Scenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p02_create_order", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var courseId = await Helpers.CreateCourse(httpClient, baseUrl, token);
            if (courseId == null) return Response.Fail(statusCode: "CreateCourseFailed");

            var payload = new
            {
                UserId = Guid.NewGuid(),
                CourseIds = new[] { courseId },
                TotalPrice = 99.99m
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-03: Izolacja zasobów i propagacja awarii (Spike Test - UploadVideo vs GetCart)
    public static ScenarioProps GetP03UploadVideoScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p03_upload_video", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var courseId = await Helpers.CreateCourse(httpClient, baseUrl, token);
            if (courseId == null) return Response.Fail(statusCode: "CreateCourseFailed");

            var fileContent = new byte[1024 * 1024];
            new Random().NextBytes(fileContent);

            var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent(courseId), "CourseId");
            multipart.Add(new ByteArrayContent(fileContent), "VideoFile", "video.mp4");

            var rawRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/courses/video")
            {
                Content = multipart
            };
            rawRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var startTime = DateTime.UtcNow;
            var response = await httpClient.SendAsync(rawRequest);
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: response.StatusCode.ToString(), sizeBytes: 1024 * 1024)
                : Response.Fail(statusCode: response.StatusCode.ToString());
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );
    }

    public static ScenarioProps GetP03GetCartScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p03_get_cart", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var userId = Helpers.ExtractUserIdFromToken(token);

            var request = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-04: Skalowalność i koszty operacyjne (Scale-Out Test)
    public static ScenarioProps GetP04ScaleOutScenario(HttpClient httpClient, string baseUrl)
    {
        return Scenario.Create("p04_scale_out", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/courses");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );
    }

    // P-05: Wydajność agregacji rozproszonych danych (API Gateway vs zapytania SQL)
    public static ScenarioProps GetP05GetOrderHistoryScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p05_get_order_history", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var courseId = await Helpers.CreateCourse(httpClient, baseUrl, token);
            if (courseId == null) return Response.Fail(statusCode: "CreateCourseFailed");

            var payload = new { CourseIds = new[] { courseId }, TotalPrice = 99.99m };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var createOrderRequest = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            var createOrderResponse = await Http.Send(httpClient, createOrderRequest);
            if (createOrderResponse.IsError) return Response.Fail(statusCode: "CreateOrderFailed");

            var orderResultString = await createOrderResponse.Payload.Value.Content.ReadAsStringAsync();
            using var orderResultDoc = JsonDocument.Parse(orderResultString);
            var orderId = orderResultDoc.RootElement.GetProperty("id").GetString();

            var getOrderRequest = Http.CreateRequest("GET", $"{baseUrl}/api/orders/{orderId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, getOrderRequest);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-06: Rywalizacja o pulę połączeń (Connection Pool Starvation)
    public static ScenarioProps GetP06HeavyDbLoadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p06_heavy_db_load", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var payload = new
            {
                Name = $"Test Course {Guid.NewGuid()}",
                Title = "Test Course",
                Description = "A great course",
                Price = 99.99m
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/courses")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    public static ScenarioProps GetP06NormalLoadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p06_normal_load", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var userId = Helpers.ExtractUserIdFromToken(token);

            var request = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-07: Narzut brokera wiadomości vs zdarzenia wewnątrzprocesowe
    public static ScenarioProps GetP07EventPropagationScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p07_event_propagation", async context =>
        {
            var token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return Response.Fail(statusCode: "LoginFailed");

            var userId = Helpers.ExtractUserIdFromToken(token);

            var courseId = await Helpers.CreateCourse(httpClient, baseUrl, token);
            if (courseId == null) return Response.Fail(statusCode: "CreateCourseFailed");

            var addToCartSuccess = await Helpers.AddToCart(httpClient, baseUrl, token, courseId, userId);
            if (!addToCartSuccess) return Response.Fail(statusCode: "AddToCartFailed");

            var payload = new { CourseIds = new[] { courseId }, TotalPrice = 99.99m };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            var startTime = DateTime.UtcNow;
            var createOrderResponse = await Http.Send(httpClient, request);
            if (createOrderResponse.IsError) return Response.Fail(statusCode: "CreateOrderFailed");

            bool eventPropagated = false;
            int maxRetries = 20;
            while (maxRetries-- > 0)
            {
                var getCartRequest = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}")
                    .WithHeader("Authorization", $"Bearer {token}");
                var getCartResponse = await Http.Send(httpClient, getCartRequest);

                if (!getCartResponse.IsError)
                {
                    var cartResultString = await getCartResponse.Payload.Value.Content.ReadAsStringAsync();
                    using var cartResultDoc = JsonDocument.Parse(cartResultString);
                    var itemsArray = cartResultDoc.RootElement.GetProperty("items");

                    if (itemsArray.GetArrayLength() == 0) // Event handler OrderCompletedCartCleaner executed
                    {
                        eventPropagated = true;
                        break;
                    }
                }
                await Task.Delay(50);
            }
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return eventPropagated
                ? Response.Ok()
                : Response.Fail(statusCode: "EventTimeout");
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-08: Koszt pamięciowy izolacji (Memory Footprint Overhead)
    public static ScenarioProps GetP08MemoryOverheadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        return Scenario.Create("p08_memory_overhead", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/courses");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60)) // low constant load
        );
    }


    // P-09: test endpointu
    public static ScenarioProps GetP09CreateUserScenario(HttpClient httpClient, string baseUrl)
    {
        return Scenario.Create("P09_Create_User", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/users");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 1, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }
}
