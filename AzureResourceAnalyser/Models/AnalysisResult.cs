namespace AzureResourceAnalyser.Models;

public class AnalysisResult
{
    public AzureResource Resource { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> Issues { get; set; } = new List<string>();
}