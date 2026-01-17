namespace AzureResourceAnalyser.Models;

public class StorageAccountResource : AzureResource
{
    public string? Sku { get; set; }
    public bool? EncryptionEnabled { get; set; }
    public string? Kind { get; set; }
}
