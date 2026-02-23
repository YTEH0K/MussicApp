using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models.UserRelated;
using MussicApp.Services.Other;
using Stripe;
using Stripe.Checkout;
public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public SubscriptionService(
        AppDbContext db,
        IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string> CreateSubscriptionSessionAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var customerService = new CustomerService();

        if (string.IsNullOrEmpty(user.StripeCustomerId))
        {
            var customer = await customerService.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = user.Email
                });

            user.StripeCustomerId = customer.Id;
            await _db.SaveChangesAsync();
        }

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            Customer = user.StripeCustomerId,
            SuccessUrl = "https://localhost:3000/success",
            CancelUrl = "https://localhost:3000/cancel",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = _config["Stripe:PremiumPriceId"],
                    Quantity = 1
                }
            ]
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signature,
            _config["Stripe:WebhookSecret"]);

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.StripeCustomerId == session.CustomerId);

            if (user != null)
            {
                user.Role = UserRole.Premium;
                user.StripeSubscriptionId =
                    session.SubscriptionId;

                await _db.SaveChangesAsync();
            }
        }

        if (stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted)
        {
            var subscription =
                stripeEvent.Data.Object as Subscription;

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.StripeSubscriptionId == subscription.Id);

            if (user != null)
            {
                user.Role = UserRole.User;
                user.StripeSubscriptionId = null;

                await _db.SaveChangesAsync();
            }
        }
    }
}