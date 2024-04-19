using Htmx;
using static Microsoft.AspNetCore.Http.Results;

namespace HxTriggerSample;

public static class HtmxEndpoints
{
    // the global JavaScript event name
    public const string LatestCount = "latest-count";
    
    // Global count. Don't do this in a production app
    // use a database or something else 
    public static int Count { get; private set; }

    public static void MapHtmxEndpoints(this WebApplication app)
    {
        app.MapGet("/current-count", CurrentCount);
        app.MapPost("/increase-count", (Delegate)IncreaseCount);
    }
    
    /// <summary>
    /// The POST method that increments the count and adds an
    /// Hx-Trigger header to the response to fire the global event
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static Task<IResult> IncreaseCount(HttpContext ctx)
    {
        Count++;
        ctx.Response.Htmx(r => r.WithTrigger(LatestCount));
        return Task.FromResult(NoContent());
    }

    /// <summary>
    /// The GET method that returns the HTML snippet for the current count
    /// </summary>
    /// <returns></returns>
    private static Task<IResult> CurrentCount()
    {
        return Task.FromResult(
            Content($"Current Count: {Count} ({DateTime.Now:g})")
        );
    }
}