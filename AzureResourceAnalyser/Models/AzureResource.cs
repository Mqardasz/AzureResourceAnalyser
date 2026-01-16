using System.Runtime.CompilerServices;

namespace AzureResourceAnalyser.Models;

public abstract class AzureResource
{
    public string Name { get; set; }
    public string ResourceType { get; set; }
    public string ResourceGroup { get; set; }
}