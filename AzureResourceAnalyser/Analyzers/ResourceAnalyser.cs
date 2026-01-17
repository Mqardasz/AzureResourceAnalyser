using AzureResourceAnalyser.Models;
using AzureResourceAnalyser.Rules;

namespace AzureResourceAnalyser.Analyzers;

public class ResourceAnalyser : IResourceAnalyser
{
    public AnalysisResult Analyze(AzureResource resource, IEnumerable<IComplianceRule> rules)
    {
        var result = new AnalysisResult { Resource = resource };

        foreach (var rule in rules)
        {
            if (!rule.IsCompliant(resource, out var issue))
            {
                result.IsCompliant = false;
                result.Issues.Add(issue);
            }
        }

        if (result.Issues.Count == 0)
        {
            result.IsCompliant = true;
        }

        return result;
    }
}