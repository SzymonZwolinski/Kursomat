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
            Password = password,
            FullName = "Test User"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
        var reqToUse = Http.CreateRequest("POST", $"{baseUrl}/api/users/register").WithHeader("Content-Type", "application/json").WithBody(registerContent);

        var registerResponse = await Http.Send(httpClient, reqToUse);
        if (registerResponse.IsError) return null;

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
        if (loginResponse.IsError) return null;

        var loginResultString = await loginResponse.Payload.Value.Content.ReadAsStringAsync();
        using var loginResultDoc = JsonDocument.Parse(loginResultString);

        var token = loginResultDoc.RootElement.GetProperty("token").GetString();
        return token;
    }

    public static string ExtractUserIdFromToken(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == "UserId" ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        return userIdClaim?.Value ?? Guid.Empty.ToString();
    }

    public static async Task<string?> CreateCourse(HttpClient httpClient, string baseUrl, string token)
    {
        var createCoursePayload = new
        {
            Name = $"Test Course {Guid.NewGuid()}",
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
        if (createCourseResponse.IsError) return null;

        var courseResultString = await createCourseResponse.Payload.Value.Content.ReadAsStringAsync();
        using var courseResultDoc = JsonDocument.Parse(courseResultString);
        return courseResultDoc.RootElement.GetProperty("id").GetString();
    }

    public static async Task<bool> AddToCart(HttpClient httpClient, string baseUrl, string token, string courseId, string userId)
    {
        var addToCartPayload = new { CourseId = courseId };
        var addToCartContent = new StringContent(JsonSerializer.Serialize(addToCartPayload), Encoding.UTF8, "application/json");
        var addToCartRequest = Http.CreateRequest("POST", $"{baseUrl}/api/carts/{userId}/items")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Authorization", $"Bearer {token}")
            .WithBody(addToCartContent);

        var addToCartResponse = await Http.Send(httpClient, addToCartRequest);
        return !addToCartResponse.IsError;
    }
}
