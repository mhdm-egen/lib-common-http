using System.Net.Http.Json;
using System.Text.Json;
using YourOrg.Common.Core.Results;
using YourOrg.Common.Http.Exceptions;

namespace YourOrg.Common.Http.Client;

/// <summary>
/// Base class for typed HTTP clients. Wraps responses in Result&lt;T&gt; so callers
/// never receive raw HttpResponseMessage or have to handle HttpRequestException directly.
/// </summary>
public abstract class ApiClientBase(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected async Task<Result<T>> GetAsync<T>(string path, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync(path, ct);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
                return value is null
                    ? Result.Failure<T>(Error.NullValue)
                    : Result.Success(value);
            }

            return MapErrorResponse(response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<T>(Error.NotFound("Http.Unavailable", ex.Message));
        }
    }

    protected async Task<Result> PostAsync<TBody>(string path, TBody body, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, ct);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : MapErrorResponse(response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure(Error.NotFound("Http.Unavailable", ex.Message));
        }
    }

    private static Result<T> MapErrorResponse<T>(System.Net.HttpStatusCode statusCode) =>
        statusCode switch
        {
            System.Net.HttpStatusCode.NotFound     => Result.Failure<T>(Error.NotFound("Http.NotFound", "Resource not found")),
            System.Net.HttpStatusCode.Unauthorized => Result.Failure<T>(Error.Unauthorized("Http.Unauthorized", "Unauthorized")),
            System.Net.HttpStatusCode.Conflict     => Result.Failure<T>(Error.Conflict("Http.Conflict", "Conflict")),
            _ => Result.Failure<T>(new Error($"Http.{(int)statusCode}", $"HTTP {(int)statusCode}"))
        };

    private static Result MapErrorResponse(System.Net.HttpStatusCode statusCode) =>
        statusCode switch
        {
            System.Net.HttpStatusCode.NotFound     => Result.Failure(Error.NotFound("Http.NotFound", "Resource not found")),
            System.Net.HttpStatusCode.Unauthorized => Result.Failure(Error.Unauthorized("Http.Unauthorized", "Unauthorized")),
            _ => Result.Failure(new Error($"Http.{(int)statusCode}", $"HTTP {(int)statusCode}"))
        };
}
