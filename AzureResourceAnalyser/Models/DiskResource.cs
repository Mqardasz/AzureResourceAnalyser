namespace AzureResourceAnalyser.Models;

public class DiskResource : AzureResource
{
    public string? Size { get; set; }
    public string? Type { get; set; }
    public string? PerformanceTier { get; set; }
}