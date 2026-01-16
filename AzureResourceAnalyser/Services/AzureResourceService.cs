namespace AzureResourceAnalyser.Services;

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
    
    
    
}