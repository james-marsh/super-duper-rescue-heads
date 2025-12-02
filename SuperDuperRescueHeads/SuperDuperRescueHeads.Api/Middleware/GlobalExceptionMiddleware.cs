using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Shared;
using System.Net;
using System.Text.Json;

namespace SuperDuperRescueHeads.Api.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to standardized ProblemDetails responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            NotFoundException notFoundEx => CreateProblemDetails(
                context,
                HttpStatusCode.NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                notFoundEx),

            ValidationException validationEx => CreateValidationProblemDetails(
                context,
                validationEx),

            UnauthorizedException unauthorizedEx => CreateProblemDetails(
                context,
                HttpStatusCode.Forbidden,
                "Forbidden",
                unauthorizedEx.Message,
                unauthorizedEx),

            ConflictException conflictEx => CreateProblemDetails(
                context,
                HttpStatusCode.Conflict,
                "Conflict",
                conflictEx.Message,
                conflictEx),

            ConcurrencyException concurrencyEx => CreateProblemDetails(
                context,
                HttpStatusCode.Conflict,
                "Concurrency Conflict",
                concurrencyEx.Message,
                concurrencyEx),

            // DbUpdateConcurrencyException must come before DbUpdateException (more specific first)
            DbUpdateConcurrencyException dbConcurrencyEx => CreateProblemDetails(
                context,
                HttpStatusCode.Conflict,
                "Concurrency Conflict",
                "The resource was modified by another user. Please reload and try again.",
                dbConcurrencyEx),

            DbUpdateException dbUpdateEx => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Database Update Failed",
                "The operation failed due to a database constraint violation",
                dbUpdateEx),

            ArgumentException argumentEx => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Invalid Argument",
                argumentEx.Message,
                argumentEx),

            InvalidOperationException invalidOpEx => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Invalid Operation",
                invalidOpEx.Message,
                invalidOpEx),

            _ => CreateProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred",
                exception)
        };

        // Log the exception
        LogException(exception, problemDetails.Status ?? 500);

        // Write response
        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail,
        Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Include stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Add trace ID for correlation
        if (context.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        }

        return problemDetails;
    }

    private ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException validationException)
    {
        var problemDetails = new ValidationProblemDetails(validationException.Errors)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred",
            Instance = context.Request.Path,
            Type = "https://httpstatuses.com/400"
        };

        // Add trace ID for correlation
        if (context.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        }

        return problemDetails;
    }

    private void LogException(Exception exception, int statusCode)
    {
        var logLevel = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            exception,
            "HTTP {StatusCode}: {ExceptionType} - {Message}",
            statusCode,
            exception.GetType().Name,
            exception.Message);
    }
}
