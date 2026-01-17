using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

/// <summary>
/// Reguła sprawdzająca czy zasoby znajdują się w dozwolonych regionach
/// </summary>
public class LocationRule : IComplianceRule
{
    private readonly HashSet<string> _allowedLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "westeurope",
        "northeurope", 
        "eastus",
        "westus"
    };

    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (!string.IsNullOrEmpty(resource.Location))
        {
            // Normalizuj nazwę lokacji (usuń spacje i zamień na małe litery)
            var normalizedLocation = resource.Location.Replace(" ", "").ToLowerInvariant();
            
            if (!_allowedLocations.Contains(normalizedLocation))
            {
                issue = $"Zasób znajduje się w niedozwolonym regionie: {resource.Location}. Dozwolone regiony: {string.Join(", ", _allowedLocations)}";
                return false;
            }
        }

        return true;
    }
}
