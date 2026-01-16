namespace AzureResourceAnalyser.Models;

public class VirtualMachineResource : AzureResource
{
    public String ImageName { get; set; }
    public String Size { get; set; }
    public String Location {get; set;}
    public String Status {get; set;}
    public String OperatingSystem { get; set; }
}