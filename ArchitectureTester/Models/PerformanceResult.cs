using System.Diagnostics;
using System.Net.Http.Json;

namespace ArchitectureTester.Services;

public class PerformanceResult<T>
{
    public T? Data { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PerformanceResult
{
    public long ElapsedMilliseconds { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
