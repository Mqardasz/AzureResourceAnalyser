using AzureResourceAnalyser.Models;

namespace AzureResourceAnalyser.Services;
using Azure.ResourceManager.Resources.Models;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

public class AzureResourceService
{
    private readonly ArmClient _armClient;

    public AzureResourceService()
    {
        var credential = new DefaultAzureCredential();
        _armClient = new ArmClient(credential);
    }

    
    // POBRANIE SUBSKRYPCJI
    public async Task<List<SubscriptionResource>> GetSubscriptionAsync()
    {
        var subscriptions = new List<SubscriptionResource>();
        await foreach (var sub in _armClient.GetSubscriptions().GetAllAsync())
        {
            subscriptions.Add(sub);
        }
        return subscriptions;
            
    }
    
    public async Task<List<ResourceGroupResource>> GetResourceGroupsAsync(
        SubscriptionResource subscription)
    {
        var resourceGroups = new List<ResourceGroupResource>();

        await foreach (var rg in subscription.GetResourceGroups().GetAllAsync())
        {
            resourceGroups.Add(rg);
        }

        return resourceGroups;
    }
    
    public async Task<List<AzureResource>> GetResourcesAsync(ResourceGroupResource resourceGroup)
    {
        var resources = new List<AzureResource>();

        await foreach (var genericResource in resourceGroup.GetGenericResourcesAsync())
        {
            AzureResource azureResource = null;

            // Sprawdź typ zasobu i rzutuj na odpowiednią klasę dziedziczącą AzureResource
            if (genericResource.Data.ResourceType.Type.Contains("virtualMachines", StringComparison.OrdinalIgnoreCase))
            {
                azureResource = new VirtualMachineResource
                {
                    Name = genericResource.Data.Name,
                    ResourceType = genericResource.Data.ResourceType.Type,
                    ResourceGroup = resourceGroup.Data.Name,
                    Location = genericResource.Data.Location
                };
            }
            else if (genericResource.Data.ResourceType.Type.Contains("disks", StringComparison.OrdinalIgnoreCase))
            {
                azureResource = new DiskResource
                {
                    Name = genericResource.Data.Name,
                    ResourceType = genericResource.Data.ResourceType.Type,
                    ResourceGroup = resourceGroup.Data.Name,
                    Location = genericResource.Data.Location,
                    PerformanceTier = genericResource.Data.Sku?.Name,
                };
            }
            else
            {
                Console.WriteLine($"[SKIP] Unknown resource type: {genericResource.Data.ResourceType.Type}");
                continue;
            }
            resources.Add(azureResource);
        }

        return resources;
    }
    
    
}