using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class DiskSizeRule : IComplianceRule
{
    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (resource is DiskResource diskResource)
        {
            if (diskResource.PerformanceTier == "Standard_LRS")
            {
                issue = $"Dysk ma niezgodny typ wydajności: {diskResource.PerformanceTier}";
                return false;
            }
        }

        return true;
    }
}