using AzureResourceAnalyser.Rules;

namespace AzureResourceAnalyser.Services;

public class RuleManager
{
    private readonly List<IComplianceRule> _rules = new List<IComplianceRule>();

    public RuleManager()
    {
        // Domyślne reguły - zawsze aktywne
        LoadDefaultRules();
    }

    private void LoadDefaultRules()
    {
        // Podstawowe reguły zgodności
        _rules.Add(new VmTagRule());
        _rules.Add(new VmSizeRule());
        _rules.Add(new SshRule());
        _rules.Add(new DiskSizeRule());
        _rules.Add(new StorageEncryptionRule());
        
        // Sprawdzanie tagów rozliczeniowych (CostCenter, Project, Owner)
        // _rules.Add(new BillingTagRule());
        
        // Sprawdzanie dozwolonych regionów Azure
        _rules.Add(new LocationRule());
        
        // Sprawdzanie konwencji nazewnictwa zasobów
        // _rules.Add(new NamingConventionRule());
    }

    public void AddRule(IComplianceRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }
        _rules.Add(rule);
    }

    public void RemoveRule(IComplianceRule rule)
    {
        _rules.Remove(rule);
    }

    public IEnumerable<IComplianceRule> GetRules()
    {
        return _rules;
    }

    public void ClearRules()
    {
        _rules.Clear();
    }
}
