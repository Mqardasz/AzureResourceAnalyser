// See https://aka.ms/new-console-template for more information

using AzureResourceAnalyser.Services;

var service = new AzureResourceService();
var subscriptions = await service.GetSubscriptionAsync();

foreach (var sub in subscriptions)
{
    Console.WriteLine(sub.Data.DisplayName);
    
}