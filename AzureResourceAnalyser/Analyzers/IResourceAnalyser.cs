using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Rules;

namespace AzureResourceAnalyser.Analyzers;

public interface IResourceAnalyser
{
    public void Analyze(AzureResource resource, IEnumerable<IComplianceRule> rules);
}