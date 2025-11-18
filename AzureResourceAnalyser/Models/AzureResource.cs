using System.Runtime.CompilerServices;

namespace AzureResourceAnalyser.Models;

public abstract class AzureResource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ResourceType { get; set; }
    public string ResourceGroup { get; set; }
}