using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Rules;

namespace AzureResourceAnalyser.Analyzers;

public interface IResourceAnalyser
{
    public AnalysisResult Analyze(AzureResource resource, IEnumerable<IComplianceRule> rules);
}