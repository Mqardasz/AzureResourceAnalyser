using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

/// <summary>
/// Reguła sprawdzająca konwencję nazewnictwa zasobów
/// Wymagany format: [typ]-[env]-[nazwa]-[numer]
/// Przykład: vm-prod-web-01, disk-dev-data-02
/// </summary>
public class NamingConventionRule : IComplianceRule
{
    private readonly HashSet<string> _validPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "vm", "disk", "storage", "app", "db", "sql", "web"
    };

    private readonly HashSet<string> _validEnvironments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "prod", "dev", "test", "staging", "uat"
    };

    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (string.IsNullOrEmpty(resource.Name))
        {
            return true; // jesli nie ma nazwy pomin
        }

        var parts = resource.Name.ToLowerInvariant().Split('-');

        // Sprawdź czy nazwa ma przynajmniej 3 części (typ-env-nazwa)
        if (parts.Length < 3)
        {
            issue = $"Nazwa zasobu '{resource.Name}' nie spełnia konwencji nazewnictwa. " +
                   $"Wymagany format: [typ]-[env]-[nazwa]-[numer], np. vm-prod-web-01";
            return false;
        }

        // Sprawdź prefix (typ zasobu)
        if (!_validPrefixes.Contains(parts[0]))
        {
            issue = $"Nazwa zasobu '{resource.Name}' zawiera nieprawidłowy prefix. " +
                   $"Dozwolone: {string.Join(", ", _validPrefixes)}";
            return false;
        }

        // Sprawdź środowisko
        if (!_validEnvironments.Contains(parts[1]))
        {
            issue = $"Nazwa zasobu '{resource.Name}' zawiera nieprawidłowe środowisko. " +
                   $"Dozwolone: {string.Join(", ", _validEnvironments)}";
            return false;
        }

        return true;
    }
}
