using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Utils;
using Renci.SshNet; // NuGet: SSH.NET
using System.Text;

namespace AzureResourceAnalyser.Reporting;

public class PrometheusExporter
{
    private string _remoteHost = "20.108.25.162";
    private string _username = "azureuser";
    private string _remoteDir = "/var/lib/node_exporter/textfile_collector";
    private string _remoteFileName = "azure_resources.prom";
    private string _privateKeyPath = @"C:\Users\micha\.ssh\ssh-key.pem";
    

    public async Task ExportMetricsAsync(List<AnalysisResult> results)
    {
        try
        {
            var metrics = new List<string>();

            // Globalne metryki
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

            // Konwertuj listę metryk na string
            var metricsText = string.Join("\n", metrics);

            // Zapisz tymczasowo lokalnie
            string tempFile = Path.Combine(Path.GetTempPath(), _remoteFileName);
            await File.WriteAllTextAsync(tempFile, string.Join("\n", metrics), new UTF8Encoding(false));

            // Wyślij plik przez SCP
            using (var scp = new ScpClient(_remoteHost, _username, new PrivateKeyFile(_privateKeyPath)))
            {
                scp.Connect();
                using (var fileStream = new FileStream(tempFile, FileMode.Open))
                {
                    scp.Upload(fileStream, $"{_remoteDir}/{_remoteFileName}");
                }
                scp.Disconnect();
            }

            Logger.LogSuccess($"Metryki Prometheus wysłane na serwer: {_remoteHost}:{_remoteDir}/{_remoteFileName}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas eksportu metryk Prometheus: {ex.Message}");
        }
    }

    private string SanitizeMetricName(string name)
    {
        return name.ToLower()
            .Replace("/", "_")
            .Replace(".", "_")
            .Replace("-", "_")
            .Replace(" ", "_");
    }
}
