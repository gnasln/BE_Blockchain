using Base_BE.Infrastructure;

namespace Base_BE.Endpoints;

public class HealthCheck : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup("HealthCheck").WithTags("Ping").MapGet("/ping", () =>
        {
            return 1;
        })
        .WithName("Ping")
        .WithOpenApi();
    }
}