using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class VmSizeRule : IComplianceRule
{
    private readonly HashSet<string> _deprecatedSizes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Basic_A0", "Basic_A1", "Basic_A2", "Basic_A3", "Basic_A4",
        "Standard_A0", "Standard_A1", "Standard_A2", "Standard_A3", "Standard_A4"
    };

    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (resource is VirtualMachineResource vm)
        {
            if (!string.IsNullOrEmpty(vm.Size) && _deprecatedSizes.Contains(vm.Size))
            {
                issue = $"Maszyna wirtualna używa przestarzałego rozmiaru: {vm.Size}";
                return false;
            }
        }

        return true;
    }
}
