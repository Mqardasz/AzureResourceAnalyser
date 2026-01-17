using System.Runtime.CompilerServices;

namespace AzureResourceAnalyser.Models;

public abstract class AzureResource
{
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceGroup { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}