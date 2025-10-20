namespace SPTarkov.Server.Services;

public class RequestTrackingMiddleware
{
    private static int _activeRequests;
    private readonly RequestDelegate _next;

    public static bool OtherRequestsActive
    {
        get
        {
            return _activeRequests > 1;
        }
    }

    public RequestTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Interlocked.Increment(ref _activeRequests);
        try
        {
            await _next(context);
        }
        finally
        {
            Interlocked.Decrement(ref _activeRequests);
        }
    }
}

public static class RequestTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTrackingMiddleware>();
    }
}
