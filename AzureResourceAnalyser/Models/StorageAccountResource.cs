namespace AzureResourceAnalyser.Models;

public class DiskResource : AzureResource
{
    public String Name { get; set; }
    public String Size { get; set; }
    public String Location { get; set; }
    public String Type { get; set; }
    public String PerformanceTier { get; set; }
}