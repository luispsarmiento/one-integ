
using Microsoft.AspNetCore.Mvc;
using OneInteg.Server.Domain.Services;

namespace OneInteg.Server.Endpoints.Subscription
{
    internal sealed class Cancel : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/{t_id}/subscription/{reference}/cancel",
                async ([FromRoute(Name = "t_id")] Guid tenantId,
                       [FromRoute(Name = "reference")] string reference,
                       [FromServices] ISubscriptionService subscriptionService) =>
            {
                var subscription = await subscriptionService.CancelSubscription(tenantId, reference);

                if (subscription == null)
                {
                    return Results.NotFound(new { message = "Subscription not found or could not be cancelled" });
                }

                return Results.Ok(subscription);
            });
        }
    }
}
