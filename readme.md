# Hx-Trigger Global Event Sample

Htmx has support for out-of-band swaps, where a payload for an element on the page can piggy-back on another Htmx response. That's great, but can become a scaling issue.

Luckily, we can use JavaScript global events to retrigger Htmx-powered elements on the page and refresh the state of the page.

This repository shows you how to do that with ASP.NET Core in 2 simple steps. 

## 0. Add Htmx To Your ASP.NET Core Site

Don't forget to add Htmx to your `_Layout.cshtml`.

```html
<script src="https://unpkg.com/htmx.org@1.9.12"></script>
```

Also add the `Htmx` package to the ASP.NET Core project.

```xml
<ItemGroup>
  <PackageReference Include="Htmx" Version="1.7.0" />
</ItemGroup>
```

## 1. Create Some Endpoints

We'll create two endpoints. One endpoint returns the current count as an HTML snippet, and the other increments the count. The incrementing endpoint purposely returns `NoContent` to show that we aren't swapping anything on the response.

```csharp
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
```

## 2. Write Some HTML

Let's wire up our Htmx Endpoints to some HTML.

```razor
@page
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}

<!-- see HtmxEndpoints.CurrentCount -->
<!-- hx-get is triggered by the server using the 'latest-count' event -->
<div class="py-4"
     hx-get="/current-count"
     hx-trigger="@HtmxEndpoints.LatestCount from:body" hx-swap="innerHTML">
    <!-- We show the latest count on the initial page load -->
    Current Count: @HtmxEndpoints.Count (waiting on click...)
</div>

<!-- See HtmxEndpoints.IncreaseCount -->
<!-- there is no return HTML, so hx-swap set to 'none' -->
<button
    hx-post="/increase-count"
    hx-swap="none"
    class="btn btn-primary">
    Update Count
</button>
```

That's it! Now you should have a working counter that uses `Hx-Trigger` to refresh the page on each increment. Any Htmx element can now listen to the global event.

> Be sure to use `from:body` to listen for global events in your `hx-trigger` usages.

## Thanks

Thanks to [Alexander Zeitler for his blog post](https://alexanderzeitler.com/articles/listening-to-htmx-hx-trigger-response-header-events-from-alpine-js/) and for helping with Htmx.NET.