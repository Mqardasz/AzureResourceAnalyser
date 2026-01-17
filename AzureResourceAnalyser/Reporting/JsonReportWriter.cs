using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Utils;
using System.Text.Json;

namespace AzureResourceAnalyser.Reporting;

public class JsonReportWriter
{
    public async Task WriteReportAsync(List<AnalysisResult> results, string filePath, bool archive = true)
    {
        try
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string outputFilePath = Path.Combine(projectDirectory, filePath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Tworzenie raportu z metadanymi
            var report = new
            {
                Timestamp = DateTime.UtcNow,
                TotalResources = results.Count,
                CompliantResources = results.Count(r => r.IsCompliant),
                NonCompliantResources = results.Count(r => !r.IsCompliant),
                Results = results
            };

            var json = JsonSerializer.Serialize(report, options);
            await File.WriteAllTextAsync(outputFilePath, json);
            Logger.LogSuccess($"Raport zapisany pomyślnie: {outputFilePath}");

            // Archiwizacja raportu jeśli włączona
            if (archive)
            {
                await ArchiveReportAsync(outputFilePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas zapisywania raportu: {ex.Message}");
        }
    }

    private Task ArchiveReportAsync(string reportPath)
    {
        try
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string archiveDirectory = Path.Combine(projectDirectory, "reports_archive");
            
            // Utwórz folder archiwum jeśli nie istnieje
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            // Nazwa pliku z timestampem
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string fileName = $"analysis_report_{timestamp}.json";
            string archivePath = Path.Combine(archiveDirectory, fileName);

            // Kopiuj raport do archiwum
            File.Copy(reportPath, archivePath, true);
            Logger.LogInfo($"Raport zarchiwizowany: {archivePath}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Nie udało się zarchiwizować raportu: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }
}
