using AzureResourceAnalyser.Models;
using System.Text.Json;

namespace AzureResourceAnalyser.Reporting;

public class JsonReportWriter
{
    public async Task WriteReportAsync(List<AnalysisResult> results, string filePath)
    {
        try
        {
            // Tworzymy absolutną ścieżkę w folderze projektu
            string projectDirectory = Directory.GetCurrentDirectory(); // Folder projektu
            string outputFilePath = Path.Combine(projectDirectory, filePath); // Ścieżka z raportem

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Serializacja wyników do formatu JSON
            var json = JsonSerializer.Serialize(results, options);

            // Tworzenie lub nadpisywanie pliku w określonej lokalizacji
            await File.WriteAllTextAsync(outputFilePath, json);

            Console.WriteLine($"[INFO] Raport zapisany pomyślnie: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Wystąpił błąd podczas zapisywania raportu: {ex.Message}");
        }
    }
}