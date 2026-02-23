namespace MussicApp.Services.Other
{
    public interface ISubscriptionService
    {
        Task<string> CreateSubscriptionSessionAsync(Guid userId);
        Task HandleWebhookAsync(string json, string signature);
    }
}
