using AzureResourceAnalyser.Services;
using AzureResourceAnalyser.Analyzers;
using AzureResourceAnalyser.Reporting;
using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Utils;
using System.Diagnostics;

Logger.LogInfo("=== Azure Resource Analyzer - Start ===");
Logger.LogInfo("Narzędzie do inwentaryzacji i analizy zasobów Azure");
Console.WriteLine();

var stopwatch = Stopwatch.StartNew();

try
{
    // Inicjalizacja serwisów
    var service = new AzureResourceService();
    var analyser = new ResourceAnalyser();
    var jsonReportWriter = new JsonReportWriter();
    var prometheusExporter = new PrometheusExporter();
    var ruleManager = new RuleManager();

    Logger.LogInfo("Inicjalizacja menedżera reguł zgodności...");
    var complianceRules = ruleManager.GetRules();
    Logger.LogSuccess($"Załadowano {complianceRules.Count()} reguł zgodności");
    Console.WriteLine();

    // Pobieranie subskrypcji
    Logger.LogInfo("Pobieranie subskrypcji Azure...");
    var subscriptions = await service.GetSubscriptionAsync();
    
    if (subscriptions.Count == 0)
    {
        Logger.LogWarning("Nie znaleziono żadnych subskrypcji. Sprawdź uprawnienia.");
        return;
    }

    Console.WriteLine();

    var analysisResults = new List<AnalysisResult>();

    // Analiza zasobów w każdej subskrypcji
    foreach (var sub in subscriptions)
    {
        Logger.LogInfo($"Przetwarzanie subskrypcji: {sub.Data.DisplayName}");

        var resourceGroups = await service.GetResourceGroupsAsync(sub);
        Logger.LogInfo($"Znaleziono {resourceGroups.Count} grup zasobów");

        foreach (var resourceGroup in resourceGroups)
        {
            Logger.LogInfo($"  Analiza grupy zasobów: {resourceGroup.Data.Name}");

            var resources = await service.GetResourcesAsync(resourceGroup);
            Logger.LogInfo($"    Znaleziono {resources.Count} zasobów");

            foreach (var resource in resources)
            {
                var result = analyser.Analyze(resource, complianceRules);
                analysisResults.Add(result);

                if (!result.IsCompliant)
                {
                    Logger.LogWarning($"    ⚠ Niezgodność: {resource.Name}");
                    foreach (var issue in result.Issues)
                    {
                        Logger.LogWarning($"      - {issue}");
                    }
                }
            }
        }
        Console.WriteLine();
    }

    // Generowanie raportów
    Logger.LogInfo("Generowanie raportów...");
    
    string jsonReportPath = "analysis_report.json";
    await jsonReportWriter.WriteReportAsync(analysisResults, jsonReportPath);
    
    await prometheusExporter.ExportMetricsAsync(analysisResults);

    Console.WriteLine();

    // Statystyki końcowe
    stopwatch.Stop();
    
    Logger.LogInfo("==========================================");
    Logger.LogInfo("         STATYSTYKI ANALIZY");
    Logger.LogInfo("==========================================");
    
    var compliantResources = analysisResults.Count(r => r.IsCompliant);
    var nonCompliantResources = analysisResults.Count(r => !r.IsCompliant);
    var complianceRate = analysisResults.Count > 0 
        ? (double)compliantResources / analysisResults.Count * 100 
        : 0;

    Console.WriteLine($"Łączna liczba zasobów:     {analysisResults.Count}");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Zasoby zgodne:             {compliantResources}");
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Zasoby niezgodne:          {nonCompliantResources}");
    Console.ResetColor();
    Console.WriteLine($"Wskaźnik zgodności:        {complianceRate:F2}%");
    Console.WriteLine();

    // Statystyki per typ zasobu
    Logger.LogInfo("Zasoby według typu:");
    var resourcesByType = analysisResults.GroupBy(r => r.Resource?.ResourceType ?? "Unknown");
    foreach (var group in resourcesByType)
    {
        var typeCompliant = group.Count(r => r.IsCompliant);
        var typeNonCompliant = group.Count(r => !r.IsCompliant);
        Console.WriteLine($"  {group.Key}:");
        Console.WriteLine($"    Zgodne: {typeCompliant}, Niezgodne: {typeNonCompliant}");
    }

    Console.WriteLine();
    Logger.LogInfo($"Czas wykonania: {stopwatch.Elapsed.TotalSeconds:F2} sekund");
    Logger.LogSuccess("Analiza zakończona pomyślnie!");
}
catch (Exception ex)
{
    Logger.LogError($"Wystąpił błąd krytyczny: {ex.Message}");
    Logger.LogError($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}
