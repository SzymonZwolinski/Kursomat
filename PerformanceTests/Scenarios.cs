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
        string token = null;
        string courseId = null;

        return Scenario.Create("p02_create_order", async context =>
        {
            if (token == null || courseId == null) return Response.Fail(statusCode: "SetupFailed");

            // BUDUJEMY PRAWIDŁOWY PAYLOAD JSON (koniec z 400 Bad Request)
            var payload = new
            {
                CourseIds = new[] { courseId },
                TotalPrice = 99.99m
            };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            var request = Http.CreateRequest("POST", $"{baseUrl.TrimEnd('/')}/api/orders")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            var response = await Http.Send(httpClient, request);

            if (response.IsError)
            {
                var err = await response.Payload.Value.Content.ReadAsStringAsync();
                Console.WriteLine($"\n[BŁĄD Zamówienia] Status: {response.StatusCode} | Body: {err}\n");
            }
            return response;
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token != null)
            {
                courseId = await Helpers.CreateCourse(httpClient, baseUrl, token);
                Console.WriteLine($"\n[DEBUG INIT] Rozgrzewka P-02 udana! Course: {courseId}\n");
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-03: Izolacja zasobów i propagacja awarii (Spike Test - UploadVideo vs GetCart)
    public static ScenarioProps GetP03UploadVideoScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string courseId = null;
        byte[] fileContent = new byte[1024 * 1024];
        new Random().NextBytes(fileContent);

        return Scenario.Create("p03_upload_video", async context =>
        {
            if (token == null || courseId == null) return Response.Fail(statusCode: "SetupFailed");

            var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent(courseId), "CourseId");
            multipart.Add(new ByteArrayContent(fileContent), "VideoFile", "video.mp4");

            var rawRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/courses/video")
            {
                Content = multipart
            };
            rawRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(rawRequest);

            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: response.StatusCode.ToString(), sizeBytes: 1024 * 1024)
                : Response.Fail(statusCode: response.StatusCode.ToString());
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token != null)
                courseId = await Helpers.CreateCourse(httpClient, baseUrl);

            // 3. KLUCZ: Dodajemy ten kurs do koszyka w bazie
                await Helpers.AddToCart(httpClient, baseUrl, token, courseId);
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
        string token = null;
        string userId = null;

        return Scenario.Create("p03_get_cart", async context =>
        {
            if (token == null || userId == null) return Response.Fail(statusCode: "SetupFailed");

            var request = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, request);
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token != null)
                userId = Helpers.ExtractUserIdFromToken(token);
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
        string token = null;
        string orderId = null;

        return Scenario.Create("p05_get_order_history", async context =>
        {
            if (token == null || orderId == null) return Response.Fail(statusCode: "SetupFailed");

            var getOrderRequest = Http.CreateRequest("GET", $"{baseUrl}/api/orders/{orderId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, getOrderRequest);
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return;

            var courseId = await Helpers.CreateCourse(httpClient, baseUrl);

            // 3. KLUCZ: Dodajemy ten kurs do koszyka w bazie
            if (courseId == null) return;
            await Helpers.AddToCart(httpClient, baseUrl, token, courseId);

            var payload = new { CourseIds = new[] { courseId }, TotalPrice = 99.99m };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var createOrderRequest = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            var createOrderResponse = await Http.Send(httpClient, createOrderRequest);
            if (!createOrderResponse.IsError)
            {
                var orderResultString = await createOrderResponse.Payload.Value.Content.ReadAsStringAsync();
                using var orderResultDoc = JsonDocument.Parse(orderResultString);
                orderId = orderResultDoc.RootElement.GetProperty("id").GetString();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-06: Rywalizacja o pulę połączeń (Connection Pool Starvation)
    public static ScenarioProps GetP06HeavyDbLoadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;

        return Scenario.Create("p06_heavy_db_load", async context =>
        {
            if (token == null) return Response.Fail(statusCode: "SetupFailed");

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
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    public static ScenarioProps GetP06NormalLoadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string userId = null;

        return Scenario.Create("p06_normal_load", async context =>
        {
            if (token == null || userId == null) return Response.Fail(statusCode: "SetupFailed");

            var request = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}")
                .WithHeader("Authorization", $"Bearer {token}");

            return await Http.Send(httpClient, request);
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token != null)
                userId = Helpers.ExtractUserIdFromToken(token);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // P-07: Narzut brokera wiadomości vs zdarzenia wewnątrzprocesowe
    public static ScenarioProps GetP07EventPropagationScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string userId = null;
        string courseId = null;

        return Scenario.Create("p07_event_propagation", async context =>
        {
            if (token == null || courseId == null) return Response.Fail(statusCode: "SetupFailed");

            var payload = new { CourseIds = new[] { courseId }, TotalPrice = 99.99m };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(content);

            var createOrderResponse = await Http.Send(httpClient, request);
            if (createOrderResponse.IsError) return Response.Fail(statusCode: "CreateOrderFailed");

            bool eventPropagated = false;
            int maxRetries = 100; // Czeka do 10 sekund (100 * 100ms)
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

                    if (itemsArray.GetArrayLength() == 0)
                    {
                        eventPropagated = true;
                        break;
                    }
                }
                await Task.Delay(100);
            }

            return eventPropagated ? Response.Ok() : Response.Fail(statusCode: "EventTimeout");
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return;

            userId = Helpers.ExtractUserIdFromToken(token);
            courseId = await Helpers.CreateCourse(httpClient, baseUrl);

            // 3. KLUCZ: Dodajemy ten kurs do koszyka w bazie
            if (courseId != null)
            {      
                await Helpers.AddToCart(httpClient, baseUrl, token, courseId);
            }
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
            var userId = Guid.NewGuid().ToString();
            var email = $"user_{userId}@test.com";
            var payload = new
            {
                Login = email,
                Email = email,
                Password = "Password123!",
                FullName = "Performance Test User"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = Http.CreateRequest("POST", $"{baseUrl}/api/users/register") // ZMIENIONO GET NA POST I ENDPOINT
                .WithHeader("Content-Type", "application/json")
                .WithBody(content);

            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)) // Lekko zwiększone obciążenie
        );
    }

    public static ScenarioProps GetP10StreamVideoScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string courseId = null;

        return Scenario.Create("p10_stream_video", async context =>
        {
            if (token == null || courseId == null) return Response.Fail(statusCode: "SetupFailed");

            // Pobieranie strumienia wideo
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/courses/{courseId}/video")
                .WithHeader("Authorization", $"Bearer {token}");

            // Oczekujemy statusu 200/206
            var response = await Http.Send(httpClient, request);
            return response.IsError ? Response.Fail() : Response.Ok(sizeBytes: response.Payload.Value.Content.Headers.ContentLength ?? 0);
        })
        .WithInit(async context =>
        {
            // Przygotowanie: logowanie, utworzenie kursu i wgranie małego wideo (np. 5MB)
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return;

            await Helpers.CreateCourse(httpClient, baseUrl);

            // 3. KLUCZ: Dodajemy ten kurs do koszyka w bazie
            await Helpers.AddToCart(httpClient, baseUrl, token, courseId);
            var fileContent = new byte[5 * 1024 * 1024]; // 5 MB plik
            new Random().NextBytes(fileContent);
            var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent(courseId), "CourseId");
            multipart.Add(new ByteArrayContent(fileContent), "VideoFile", "video.mp4");

            var uploadReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/courses/video") { Content = multipart };
            uploadReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            await httpClient.SendAsync(uploadReq);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Mniej requestów, bo to ciężki I/O test
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    public static ScenarioProps GetP11DashboardLoadScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string userId = null;

        return Scenario.Create("p11_dashboard_load", async context =>
        {
            if (token == null || userId == null) return Response.Fail(statusCode: "SetupFailed");

            // Generujemy 3 zapytania równolegle (tak jak zrobiłaby to przeglądarka)
            var req1 = Http.CreateRequest("GET", $"{baseUrl}/api/users/{userId}").WithHeader("Authorization", $"Bearer {token}");
            var req2 = Http.CreateRequest("GET", $"{baseUrl}/api/courses").WithHeader("Authorization", $"Bearer {token}");
            var req3 = Http.CreateRequest("GET", $"{baseUrl}/api/carts/{userId}").WithHeader("Authorization", $"Bearer {token}");

            var task1 = Http.Send(httpClient, req1);
            var task2 = Http.Send(httpClient, req2);
            var task3 = Http.Send(httpClient, req3);

            // Czekamy na wszystkie 3
            var results = await Task.WhenAll(task1, task2, task3);

            // Jeśli którekolwiek z nich zawiodło, test oblewa
            if (results.Any(r => r.IsError)) return Response.Fail();

            return Response.Ok();
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token != null) userId = Helpers.ExtractUserIdFromToken(token);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    public static ScenarioProps GetP12CartConcurrencyScenario(HttpClient httpClient, string baseUrl, string targetName)
    {
        string token = null;
        string userId = null;
        string courseId = null;
        bool isAdding = true; // Zmienna do flip-flopa

        return Scenario.Create("p12_cart_concurrency", async context =>
        {
            if (token == null || courseId == null) return Response.Fail();

            HttpRequestMessage request;

            // Naprzemiennie dodajemy i usuwamy ten sam przedmiot
            if (isAdding)
            {
                var payload = new { CourseId = courseId };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                request = Http.CreateRequest("POST", $"{baseUrl}/api/carts/{userId}/items")
                    .WithHeader("Authorization", $"Bearer {token}").WithHeader("Content-Type", "application/json").WithBody(content);
            }
            else
            {
                request = Http.CreateRequest("DELETE", $"{baseUrl}/api/carts/{userId}/items/{courseId}")
                    .WithHeader("Authorization", $"Bearer {token}");
            }

            isAdding = !isAdding; // Odwracamy akcję dla kolejnego żądania

            return await Http.Send(httpClient, request);
        })
        .WithInit(async context =>
        {
            token = await Helpers.RegisterAndLogin(httpClient, baseUrl, targetName);
            if (token == null) return;

            userId = Helpers.ExtractUserIdFromToken(token);
            await Helpers.CreateCourse(httpClient, baseUrl);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Strzelamy mocno i szybko, żeby sprawdzić jak baza radzi sobie z blokadami wierszy (Locks)
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }
}
