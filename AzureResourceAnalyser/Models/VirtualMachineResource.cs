namespace AzureResourceAnalyser.Models;

public class VirtualMachineResource : AzureResource
{
    public string? ImageName { get; set; }
    public string? Size { get; set; }
    public string? Status { get; set; }
    public string? OperatingSystem { get; set; }
}