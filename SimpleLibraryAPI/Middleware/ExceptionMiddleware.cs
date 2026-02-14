public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next; // This is the "Bridge" to the next part of the app
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Try to run the Controller code
        }
        catch (Exception ex)
        {
            // IF ANYTHING BREAKS, WE CATCH IT HERE
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        // In a real app, you would write 'exception.Message' to a log file here
        return context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error. The bridge is being repaired!"
        });
    }
}