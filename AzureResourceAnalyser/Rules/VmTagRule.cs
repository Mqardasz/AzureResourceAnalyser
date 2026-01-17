using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class VmTagRule : IComplianceRule
{
    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (resource is VirtualMachineResource vm)
        {
            // Sprawdzanie czy VM ma wymagany tag "env"
            if (!vm.Tags.ContainsKey("env") || string.IsNullOrEmpty(vm.Tags["env"]))
            {
                issue = "Maszyna wirtualna nie ma wymaganego tagu 'env'.";
                return false;
            }
        }

        return true;
    }
}