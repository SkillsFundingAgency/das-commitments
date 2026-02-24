using Microsoft.AspNetCore.Http;

namespace SFA.DAS.CommitmentsV2.Api.Middleware;

public class RequestTraceMiddleware(RequestDelegate next, ILogger<RequestTraceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "(null)";
        var traceId = context.TraceIdentifier;
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        logger.LogInformation(
            "Received request: {Method} {Path} TraceId={TraceId} CorrelationId={CorrelationId}",
            method,
            path,
            traceId,
            correlationId ?? "(none)");

        await next(context);
    }
}
