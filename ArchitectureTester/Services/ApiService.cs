using System.Diagnostics;
using System.Net.Http.Json;
using ArchitectureTester.Models;

namespace ArchitectureTester.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    // Domyślne porty z plików docker-compose
    private const string MonolithUrl = "https://localhost:7044";
    private const string ModularMonolithUrl = "http://localhost:8081";
    private const string MicroservicesUrl = "http://localhost:8080";

    public ArchitectureType CurrentArchitecture { get; set; } = ArchitectureType.Monolith;
    public string? Token { get; set; }

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private string GetBaseUrl()
    {
        return CurrentArchitecture switch
        {
            ArchitectureType.Monolith => MonolithUrl,
            ArchitectureType.ModularMonolith => ModularMonolithUrl,
            ArchitectureType.Microservices => MicroservicesUrl,
            _ => MonolithUrl
        };
    }

    private string BuildUrl(string endpoint)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/{endpoint.TrimStart('/')}";
    }

    private void EnsureAuthorizationHeader()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<PerformanceResult<T>> GetAsync<T>(string endpoint)
    {
        EnsureAuthorizationHeader();
        var result = new PerformanceResult<T>();
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(BuildUrl(endpoint));
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<T>();
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Status: {response.StatusCode}. {await response.Content.ReadAsStringAsync()}";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<PerformanceResult<TResponse>> PostWithResponseAsync<TResponse, TRequest>(string endpoint, TRequest data)
    {
        EnsureAuthorizationHeader();
        var result = new PerformanceResult<TResponse>();
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), data);
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<TResponse>();
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Status: {response.StatusCode}. {await response.Content.ReadAsStringAsync()}";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<PerformanceResult> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        EnsureAuthorizationHeader();
        var result = new PerformanceResult();
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), data);
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Status: {response.StatusCode}. {await response.Content.ReadAsStringAsync()}";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<PerformanceResult> DeleteAsync(string endpoint)
    {
        EnsureAuthorizationHeader();
        var result = new PerformanceResult();
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.DeleteAsync(BuildUrl(endpoint));
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                result.IsSuccess = true;
            }
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Status: {response.StatusCode}. {await response.Content.ReadAsStringAsync()}";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}
