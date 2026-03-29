using Microsoft.Extensions.Configuration;
using NBomber.Contracts;
using NBomber.CSharp;

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
        var scenarioToRun = config["ScenarioToRun"]; // Optional, allows selecting a specific test, e.g., "P-01"

        if (string.IsNullOrEmpty(baseUrl))
        {
            Console.WriteLine("Invalid target configuration");
            return;
        }

        Console.WriteLine($"Starting Performance Tests against {targetName} at {baseUrl}");
        if (!string.IsNullOrEmpty(scenarioToRun))
        {
            Console.WriteLine($"Running specific scenario: {scenarioToRun}");
        }

        var httpClient = new HttpClient();

        var scenarios = new List<ScenarioProps>();

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-01")
            scenarios.Add(Scenarios.GetP01Scenario(httpClient, baseUrl));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-02")
            scenarios.Add(Scenarios.GetP02Scenario(httpClient, baseUrl, targetName!));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-03")
        {
            scenarios.Add(Scenarios.GetP03UploadVideoScenario(httpClient, baseUrl, targetName!));
            scenarios.Add(Scenarios.GetP03GetCartScenario(httpClient, baseUrl, targetName!));
        }

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-04")
            scenarios.Add(Scenarios.GetP04ScaleOutScenario(httpClient, baseUrl));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-05")
            scenarios.Add(Scenarios.GetP05GetOrderHistoryScenario(httpClient, baseUrl, targetName!));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-06")
        {
            scenarios.Add(Scenarios.GetP06HeavyDbLoadScenario(httpClient, baseUrl, targetName!));
            scenarios.Add(Scenarios.GetP06NormalLoadScenario(httpClient, baseUrl, targetName!));
        }

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-07")
            scenarios.Add(Scenarios.GetP07EventPropagationScenario(httpClient, baseUrl, targetName!));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-08")
            scenarios.Add(Scenarios.GetP08MemoryOverheadScenario(httpClient, baseUrl, targetName!));

        if (string.IsNullOrEmpty(scenarioToRun) || scenarioToRun == "P-09")
            scenarios.Add(Scenarios.GetP09CreateUserScenario(httpClient, baseUrl));

        NBomberRunner
            .RegisterScenarios(scenarios.ToArray())
            .WithReportFileName($"performance_report_{targetName}_{(scenarioToRun ?? "all")}")
            .WithReportFolder("./reports")
            .Run(); // removed WithReportFormats which seems changed in this NBomber version
    }
}
