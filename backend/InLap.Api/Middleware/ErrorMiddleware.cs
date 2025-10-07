using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InLap.Api.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorMiddleware> _logger;

        public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(ex, "Unhandled exception after response started");
                throw new Exception("Internal server error");
            }

            var (status, title) = MapExceptionToStatus(ex);
            var traceId = context.TraceIdentifier;
            var path = context.Request?.Path.Value ?? string.Empty;

            logger.LogError(ex, "HTTP {StatusCode} error at {Path} (traceId={TraceId})", status, path, traceId);

            var problem = new ProblemDetails
            {
                Status = (int)status,
                Title = title,
                Detail = ex.Message,
                Instance = path,
                Type = "about:blank"
            };
            problem.Extensions["traceId"] = traceId;

            var payload = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            context.Response.Clear();
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(payload);
        }

        private static (HttpStatusCode status, string title) MapExceptionToStatus(Exception ex)
        {
            if (ex is BadHttpRequestException bhre)
            {
                var code = (HttpStatusCode)bhre.StatusCode;
                if (code == HttpStatusCode.RequestEntityTooLarge)
                    return (HttpStatusCode.RequestEntityTooLarge, "Request entity too large");
                if (code == HttpStatusCode.UnsupportedMediaType)
                    return (HttpStatusCode.UnsupportedMediaType, "Unsupported media type");
                return (HttpStatusCode.BadRequest, "Bad request");
            }

            if (ex is InvalidDataException)
                return (HttpStatusCode.BadRequest, "Invalid or malformed request body");

            return (HttpStatusCode.InternalServerError, "An unexpected error occurred");
        }
    }
}
