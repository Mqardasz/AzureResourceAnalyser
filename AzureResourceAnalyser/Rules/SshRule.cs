using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class SshRule : IComplianceRule
{
    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = null;

        if (resource is VirtualMachineResource vm)
        {
            if (vm.OperatingSystem == "Linux" && !vm.Status.Contains("SSH", StringComparison.OrdinalIgnoreCase))
            {
                issue = "Maszyna wirtualna Linux ma problemy z dostępnością SSH.";
                return false;
            }
        }

        return true;
    }
}