using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class DiskSizeRule : IComplianceRule
{
    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = null;

        if (resource is DiskResource storageAccountResource)
        {
            if (storageAccountResource.PerformanceTier == "Standard LRS")
            {
                issue = $"Dysk ma niezgodny performancetier: {storageAccountResource.PerformanceTier}";
                return false;
            }
        }

        return true;
    }
}