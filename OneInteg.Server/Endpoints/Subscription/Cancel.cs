
using Microsoft.AspNetCore.Mvc;

namespace OneInteg.Server.Endpoints.Subscription
{
    internal sealed class Cancel : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/{t_id}/subscription/{reference}/cancel", 
                async ([FromRoute(Name = "t_id")] Guid tenantId,
                       [FromRoute(Name = "reference")] string reference) =>
            {
                return Results.Ok("Mock: Subscription cancelled");
            });
        }
    }
}
