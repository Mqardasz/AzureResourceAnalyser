using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Services;
using Azure.ResourceManager.Resources.Models;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureResourceAnalyser.Utils;

public class AzureResourceService
{
    private readonly ArmClient _armClient;

    public AzureResourceService()
    {
        try
        {
            var credential = new DefaultAzureCredential();
            _armClient = new ArmClient(credential);
            Logger.LogInfo("Połączono z Azure przy użyciu DefaultAzureCredential");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas uwierzytelniania: {ex.Message}");
            throw;
        }
    }

    
    // POBRANIE SUBSKRYPCJI
    public async Task<List<SubscriptionResource>> GetSubscriptionAsync()
    {
        var subscriptions = new List<SubscriptionResource>();
        try
        {
            await foreach (var sub in _armClient.GetSubscriptions().GetAllAsync())
            {
                subscriptions.Add(sub);
                Logger.LogInfo($"Znaleziono subskrypcję: {sub.Data.DisplayName} (ID: {sub.Data.SubscriptionId})");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas pobierania subskrypcji: {ex.Message}");
        }
        return subscriptions;
            
    }
    
    public async Task<List<ResourceGroupResource>> GetResourceGroupsAsync(
        SubscriptionResource subscription)
    {
        var resourceGroups = new List<ResourceGroupResource>();

        try
        {
            await foreach (var rg in subscription.GetResourceGroups().GetAllAsync())
            {
                resourceGroups.Add(rg);
                Logger.LogDebug($"  Grupa zasobów: {rg.Data.Name}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas pobierania grup zasobów: {ex.Message}");
        }

        return resourceGroups;
    }
    
    public async Task<List<AzureResource>> GetResourcesAsync(ResourceGroupResource resourceGroup)
    {
        var resources = new List<AzureResource>();

        try
        {
            await foreach (var genericResource in resourceGroup.GetGenericResourcesAsync())
            {
                AzureResource? azureResource = null;

                // Konwersja tagów do Dictionary
                var tags = new Dictionary<string, string>();
                if (genericResource.Data.Tags != null)
                {
                    foreach (var tag in genericResource.Data.Tags)
                    {
                        tags[tag.Key] = tag.Value;
                    }
                }

                // Sprawdź typ zasobu i rzutuj na odpowiednią klasę
                if (genericResource.Data.ResourceType.Type.Contains("virtualMachines", StringComparison.OrdinalIgnoreCase) &&
                    !genericResource.Data.ResourceType.Type.Contains("extensions", StringComparison.OrdinalIgnoreCase))
                {
                    azureResource = new VirtualMachineResource
                    {
                        Name = genericResource.Data.Name,
                        ResourceType = genericResource.Data.ResourceType.Type,
                        ResourceGroup = resourceGroup.Data.Name,
                        Location = genericResource.Data.Location.ToString(),
                        Tags = tags,
                        Size = genericResource.Data.Sku?.Name
                    };
                    Logger.LogDebug($"    Zasób VM: {genericResource.Data.Name}");
                }
                else if (genericResource.Data.ResourceType.Type.Contains("disks", StringComparison.OrdinalIgnoreCase))
                {
                    azureResource = new DiskResource
                    {
                        Name = genericResource.Data.Name,
                        ResourceType = genericResource.Data.ResourceType.Type,
                        ResourceGroup = resourceGroup.Data.Name,
                        Location = genericResource.Data.Location.ToString(),
                        Tags = tags,
                        PerformanceTier = genericResource.Data.Sku?.Name,
                    };
                    Logger.LogDebug($"    Zasób Disk: {genericResource.Data.Name}");
                }
                else if (genericResource.Data.ResourceType.Type.Contains("storageAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    azureResource = new StorageAccountResource
                    {
                        Name = genericResource.Data.Name,
                        ResourceType = genericResource.Data.ResourceType.Type,
                        ResourceGroup = resourceGroup.Data.Name,
                        Location = genericResource.Data.Location.ToString(),
                        Tags = tags,
                        Sku = genericResource.Data.Sku?.Name,
                        Kind = genericResource.Data.Kind,
                        EncryptionEnabled = true // Domyślnie w Azure szyfrowanie jest włączone
                    };
                    Logger.LogDebug($"    Zasób Storage: {genericResource.Data.Name}");
                }
                else if (genericResource.Data.ResourceType.Type.Contains("sites", StringComparison.OrdinalIgnoreCase))
                {
                    azureResource = new AppServiceResource
                    {
                        Name = genericResource.Data.Name,
                        ResourceType = genericResource.Data.ResourceType.Type,
                        ResourceGroup = resourceGroup.Data.Name,
                        Location = genericResource.Data.Location.ToString(),
                        Tags = tags,
                        Sku = genericResource.Data.Sku?.Name,
                        Kind = genericResource.Data.Kind
                    };
                    Logger.LogDebug($"    Zasób App Service: {genericResource.Data.Name}");
                }
                else if (genericResource.Data.ResourceType.Type.Contains("servers/databases", StringComparison.OrdinalIgnoreCase))
                {
                    azureResource = new DatabaseResource
                    {
                        Name = genericResource.Data.Name,
                        ResourceType = genericResource.Data.ResourceType.Type,
                        ResourceGroup = resourceGroup.Data.Name,
                        Location = genericResource.Data.Location.ToString(),
                        Tags = tags,
                        Sku = genericResource.Data.Sku?.Name
                    };
                    Logger.LogDebug($"    Zasób Database: {genericResource.Data.Name}");
                }
                else
                {
                    Logger.LogDebug($"    [SKIP] Nieznany typ zasobu: {genericResource.Data.ResourceType.Type}");
                    continue;
                }
                
                if (azureResource != null)
                {
                    resources.Add(azureResource);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Błąd podczas pobierania zasobów z grupy {resourceGroup.Data.Name}: {ex.Message}");
        }

        return resources;
    }
    
    
}