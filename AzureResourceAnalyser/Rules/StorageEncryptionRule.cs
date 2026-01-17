using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Rules;

public class StorageEncryptionRule : IComplianceRule
{
    public bool IsCompliant(AzureResource resource, out string issue)
    {
        issue = string.Empty;

        if (resource is StorageAccountResource storage)
        {
            if (storage.EncryptionEnabled == false || storage.EncryptionEnabled == null)
            {
                issue = "Konto Storage nie ma włączonego szyfrowania.";
                return false;
            }
        }

        return true;
    }
}
