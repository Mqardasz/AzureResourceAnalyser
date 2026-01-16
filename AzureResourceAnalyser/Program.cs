using AzureResourceAnalyser.Services;

var service = new AzureResourceService();

var subscriptions = await service.GetSubscriptionAsync();

foreach (var sub in subscriptions)
{
    Console.WriteLine($"Subscription: {sub.Data.DisplayName}");

    var resourceGroups = await service.GetResourceGroupsAsync(sub);

    foreach (var resourceGroup in resourceGroups)
    {
        Console.WriteLine($"  Resource Group: {resourceGroup.Data.Name}");
    }
}