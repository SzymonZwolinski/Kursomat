using System.Text;
using System.Text.Json;
using NBomber.Http;
using NBomber.Http.CSharp;

namespace PerformanceTests;

public static class Helpers
{
    public static async Task<string?> RegisterAndLogin(HttpClient httpClient, string baseUrl, string targetName)
    {
        var userId = Guid.NewGuid().ToString();
        var email = $"user_{userId}@test.com";
        var password = "Password123!";

        var registerPayload = new
        {
            Login = email,
            Email = email,
            Password = password
            // Usuniêto FullName, bo CreateRequest go nie przyjmuje!
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
        var reqToUse = Http.CreateRequest("POST", $"{baseUrl}/api/users/register")
            .WithHeader("Content-Type", "application/json")
            .WithBody(registerContent);

        var registerResponse = await Http.Send(httpClient, reqToUse);
        if (registerResponse.IsError)
        {
            Console.WriteLine($"\n[B£¥D Register] Status: {registerResponse.StatusCode}\n");
            return null;
        }

        var loginPayload = new
        {
            Login = email,
            Password = password
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
        var loginRequest = Http.CreateRequest("POST", $"{baseUrl}/api/users/login")
            .WithHeader("Content-Type", "application/json")
            .WithBody(loginContent);

        var loginResponse = await Http.Send(httpClient, loginRequest);
        if (loginResponse.IsError)
        {
            Console.WriteLine($"\n[B£¥D Login] Status: {loginResponse.StatusCode}\n");
            return null;
        }

        var loginResultString = await loginResponse.Payload.Value.Content.ReadAsStringAsync();
        using var loginResultDoc = JsonDocument.Parse(loginResultString);

        return loginResultDoc.RootElement.GetProperty("token").GetString();
    }

    public static string ExtractUserIdFromToken(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // KLUCZOWE: Szukamy "nameid", bo to masz w swoim JWT!
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == "nameid" ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (userIdClaim == null) Console.WriteLine("\n[B£¥D Tokena] Nie znaleziono ID u¿ytkownika!\n");

        return userIdClaim?.Value ?? Guid.Empty.ToString();
    }

    public static async Task<string?> CreateCourse(HttpClient httpClient, string baseUrl, string token = null)
    {
        var createCoursePayload = new
        {
            Name = $"Test Course {Guid.NewGuid()}",
            // Usuniêto pole "Title", bo wywala³o b³¹d API!
            Description = "A great course",
            Price = 99.99m
        };

        var createCourseContent = new StringContent(JsonSerializer.Serialize(createCoursePayload), Encoding.UTF8, "application/json");
        var createCourseRequest = Http.CreateRequest("POST", $"{baseUrl}/api/courses")
            .WithHeader("Content-Type", "application/json")
            .WithBody(createCourseContent);

        if (!string.IsNullOrEmpty(token))
        {
            createCourseRequest = createCourseRequest.WithHeader("Authorization", $"Bearer {token}");
        }

        var createCourseResponse = await Http.Send(httpClient, createCourseRequest);
        if (createCourseResponse.IsError)
        {
            var err = await createCourseResponse.Payload.Value.Content.ReadAsStringAsync();
            Console.WriteLine($"\n[B£¥D CreateCourse] Status: {createCourseResponse.StatusCode} | {err}\n");
            return null;
        }

        var courseResultString = await createCourseResponse.Payload.Value.Content.ReadAsStringAsync();
        using var courseResultDoc = JsonDocument.Parse(courseResultString);
        return courseResultDoc.RootElement.GetProperty("id").GetString();
    }

    public static async Task<bool> AddToCart(HttpClient httpClient, string baseUrl, string token, string courseId, string userId = null)
    {
        baseUrl = baseUrl.TrimEnd('/'); // Zabezpieczenie przed podwójnym slashem w URL!
        if (string.IsNullOrEmpty(userId))
        {
            userId = ExtractUserIdFromToken(token);
        }

        var url = $"{baseUrl}/api/carts/{userId}/items";
        var addToCartPayload = new { CourseId = courseId };
        var addToCartContent = new StringContent(JsonSerializer.Serialize(addToCartPayload), System.Text.Encoding.UTF8, "application/json");

        var addToCartRequest = Http.CreateRequest("POST", url)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", $"Bearer {token}")
            .WithBody(addToCartContent);

        var addToCartResponse = await Http.Send(httpClient, addToCartRequest);
        if (addToCartResponse.IsError)
        {
            var err = await addToCartResponse.Payload.Value.Content.ReadAsStringAsync();
            // TEN PRINT ZDRADZI NAM WSZYSTKO:
            Console.WriteLine($"\n[B£¥D AddToCart] URL: {url} | CourseId: {courseId} | Status: {addToCartResponse.StatusCode} | Body: {err}\n");
            return false;
        }
        return true;
    }
}