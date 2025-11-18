namespace AzureResourceAnalyser.Models;

public class VirtualMachineResource : AzureResource
{
    public String ImageName { get; set; }
    public String Size { get; set; }
}