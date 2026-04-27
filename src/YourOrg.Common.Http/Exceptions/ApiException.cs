namespace YourOrg.Common.Http.Exceptions;

public sealed class ApiException(string message, int statusCode, string? detail = null)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? Detail { get; } = detail;

    public static ApiException NotFound(string resource, string id) =>
        new($"{resource} '{id}' was not found.", 404);

    public static ApiException Unauthorized() =>
        new("Unauthorized.", 401);

    public static ApiException ServiceUnavailable(string serviceName) =>
        new($"Service '{serviceName}' is unavailable.", 503);
}
