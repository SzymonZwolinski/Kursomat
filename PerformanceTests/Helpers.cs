using System.Text;
using System.Text.Json;
using System.Net.Http.Headers; // <-- To jest kluczowe dla czystych nag³ówków
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

        var registerPayload = new { Login = email, Email = email, Password = password };
        var registerContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");

        // Czysty request C# zamiast NBomberowego
        var reqToUse = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/users/register") { Content = registerContent };
        var registerResponse = await httpClient.SendAsync(reqToUse);

        if (!registerResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"\n[B£¥D Register] Status: {registerResponse.StatusCode}\n");
            return null;
        }

        var loginPayload = new { Login = email, Password = password };
        var loginContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/users/login") { Content = loginContent };

        var loginResponse = await httpClient.SendAsync(loginRequest);
        if (!loginResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"\n[B£¥D Login] Status: {loginResponse.StatusCode}\n");
            return null;
        }

        var loginResultString = await loginResponse.Content.ReadAsStringAsync();
        using var loginResultDoc = JsonDocument.Parse(loginResultString);
        return loginResultDoc.RootElement.GetProperty("token").GetString();
    }

    public static string ExtractUserIdFromToken(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

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
            Description = "A great course",
            Price = 99.99m
        };

        var createCourseContent = new StringContent(JsonSerializer.Serialize(createCoursePayload), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/courses") { Content = createCourseContent };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n[B£¥D CreateCourse] Status: {response.StatusCode} | Body: {err}\n");
            return null;
        }

        var courseResultString = await response.Content.ReadAsStringAsync();
        using var courseResultDoc = JsonDocument.Parse(courseResultString);
        return courseResultDoc.RootElement.GetProperty("id").GetString();
    }

    public static async Task<bool> AddToCart(HttpClient httpClient, string baseUrl, string token, string courseId, string userId = null)
    {
        baseUrl = baseUrl.TrimEnd('/');
        if (string.IsNullOrEmpty(userId))
        {
            userId = ExtractUserIdFromToken(token);
        }

        var addToCartPayload = new { CourseId = courseId };
        var content = new StringContent(JsonSerializer.Serialize(addToCartPayload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/carts/{userId}/items") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n[B£¥D AddToCart] URL: {request.RequestUri} | Status: {response.StatusCode} | Body: {err}\n");
            return false;
        }
        return true;
    }
}