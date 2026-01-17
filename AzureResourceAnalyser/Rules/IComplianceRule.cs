using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public interface IComplianceRule
{
    bool IsCompliant(AzureResource resource, out string issue);
}