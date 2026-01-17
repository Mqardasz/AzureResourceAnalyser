namespace AzureResourceAnalyser.Models;

public class DatabaseResource : AzureResource
{
    public string? Sku { get; set; }
    public string? Status { get; set; }
    public string? Edition { get; set; }
}
