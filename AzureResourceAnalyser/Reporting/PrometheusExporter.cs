using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Utils;

namespace AzureResourceAnalyser.Reporting;

public class PrometheusExporter
{
    public async Task ExportMetricsAsync(List<AnalysisResult> results, string filePath)
    {
        try
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string outputFilePath = Path.Combine(projectDirectory, filePath);

            var metrics = new List<string>();
            
            // Generowanie metryk w formacie Prometheus
            metrics.Add("# HELP azure_resources_total Total number of Azure resources");
            metrics.Add("# TYPE azure_resources_total gauge");
            metrics.Add($"azure_resources_total {results.Count}");
            metrics.Add("");

            metrics.Add("# HELP azure_resources_compliant Number of compliant Azure resources");
            metrics.Add("# TYPE azure_resources_compliant gauge");
            metrics.Add($"azure_resources_compliant {results.Count(r => r.IsCompliant)}");
            metrics.Add("");

            metrics.Add("# HELP azure_resources_noncompliant Number of non-compliant Azure resources");
            metrics.Add("# TYPE azure_resources_noncompliant gauge");
            metrics.Add($"azure_resources_noncompliant {results.Count(r => !r.IsCompliant)}");
            metrics.Add("");

            // Metryki per typ zasobu
            var resourcesByType = results.GroupBy(r => r.Resource?.ResourceType ?? "Unknown");
            foreach (var group in resourcesByType)
            {
                var sanitizedType = SanitizeMetricName(group.Key);
                metrics.Add($"# HELP azure_resources_by_type_{sanitizedType} Number of resources of type {group.Key}");
                metrics.Add($"# TYPE azure_resources_by_type_{sanitizedType} gauge");
                metrics.Add($"azure_resources_by_type_{sanitizedType} {group.Count()}");
                metrics.Add("");

                var compliantInType = group.Count(r => r.IsCompliant);
                metrics.Add($"# HELP azure_resources_compliant_{sanitizedType} Compliant resources of type {group.Key}");
                metrics.Add($"# TYPE azure_resources_compliant_{sanitizedType} gauge");
                metrics.Add($"azure_resources_compliant_{sanitizedType} {compliantInType}");
                metrics.Add("");
            }

            // Zapisz metryki do pliku
            await File.WriteAllLinesAsync(outputFilePath, metrics);
            Logger.LogSuccess($"Metryki Prometheus zapisane: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas eksportu metryk Prometheus: {ex.Message}");
        }
    }

    private string SanitizeMetricName(string name)
    {
        // Zamień niedozwolone znaki w nazwach metryk Prometheus
        return name.ToLower()
            .Replace("/", "_")
            .Replace(".", "_")
            .Replace("-", "_")
            .Replace(" ", "_");
    }
}
