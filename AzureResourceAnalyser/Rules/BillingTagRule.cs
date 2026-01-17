using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

/// <summary>
/// Reguła sprawdzająca czy zasoby mają wymagane tagi dla celów rozliczeń
/// </summary>
public class BillingTagRule : IComplianceRule
{
    private readonly string[] _requiredTags = { "CostCenter", "Project", "Owner" };

    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;
        var missingTags = new List<string>();

        foreach (var requiredTag in _requiredTags)
        {
            if (!resource.Tags.ContainsKey(requiredTag) || string.IsNullOrEmpty(resource.Tags[requiredTag]))
            {
                missingTags.Add(requiredTag);
            }
        }

        if (missingTags.Any())
        {
            issue = $"Zasób nie ma wymaganych tagów rozliczeniowych: {string.Join(", ", missingTags)}";
            return false;
        }

        return true;
    }
}
