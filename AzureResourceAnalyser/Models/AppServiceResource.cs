namespace AzureResourceAnalyser.Models;

public class AppServiceResource : AzureResource
{
    public string? Sku { get; set; }
    public string? State { get; set; }
    public string? Kind { get; set; }
}
