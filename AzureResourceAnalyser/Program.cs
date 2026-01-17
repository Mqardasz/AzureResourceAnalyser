using AzureResourceAnalyser.Services;
using AzureResourceAnalyser.Analyzers;
using AzureResourceAnalyser.Reporting;
using AzureResourceAnalyser.Rules;
using AzureResourceAnalyser.Models;
using System.Text.Json;

var service = new AzureResourceService();
var analyser = new ResourceAnalyser();
var jsonReportWriter = new JsonReportWriter();

var subscriptions = await service.GetSubscriptionAsync();

var complianceRules = new List<IComplianceRule>
{
    new VmTagRule(),
    new SshRule(),
    new DiskSizeRule()
};

var analysisResults = new List<AnalysisResult>();

foreach (var sub in subscriptions)
{
    Console.WriteLine($"Subscription: {sub.Data.DisplayName}");

    var resourceGroups = await service.GetResourceGroupsAsync(sub);

    foreach (var resourceGroup in resourceGroups)
    {
        Console.WriteLine($"  Resource Group: {resourceGroup.Data.Name}");

        var resources = await service.GetResourcesAsync(resourceGroup);

        foreach (var resource in resources)
        {
            var result = analyser.Analyze(resource, complianceRules);
            analysisResults.Add(result);
        }
    }
}

// Zapisanie wyników analizy jako JSON
string jsonReportPath = "analysis_report.json";
await jsonReportWriter.WriteReportAsync(analysisResults, jsonReportPath);
Console.WriteLine($"Analiza zapisana w raporcie JSON: {jsonReportPath}");

// Statystyki
var compliantResources = analysisResults.Count(r => r.IsCompliant);
var nonCompliantResources = analysisResults.Count(r => !r.IsCompliant);

Console.WriteLine("STATYSTYKI ANALIZY:");
Console.WriteLine($"Zasoby zgodne: {compliantResources}");
Console.WriteLine($"Zasoby niezgodne: {nonCompliantResources}");
Console.WriteLine($"Łączna liczba zasobów: {analysisResults.Count}");