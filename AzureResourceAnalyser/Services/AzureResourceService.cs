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

    public async Task<List<SubscriptionResource>> GetSubscriptionAsync()
    {
        var subscriptions = new List<SubscriptionResource>();
        await foreach (var sub in _armClient.GetSubscriptions().GetAllAsync())
        {
            subscriptions.Add(sub);
        }
        return subscriptions;
            
    }
    
}