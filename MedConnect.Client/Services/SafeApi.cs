using System.Net.Http.Json;
using System.Text.Json;

namespace MedConnect.Client.Services;

public static class SafeApi
{
    public const string UnreachableMessage = "Cannot reach the server. Check your connection and try again.";

    public static async Task<ApiResult<T>> PostAsync<TBody, T>(HttpClient http, string url, TBody body) =>
        await SendAsync<T>(http, () => http.PostAsJsonAsync(url, body));

    public static async Task<ApiResult<T>> PutAsync<TBody, T>(HttpClient http, string url, TBody body) =>
        await SendAsync<T>(http, () => http.PutAsJsonAsync(url, body));

    public static async Task<ApiResult<T>> SendAsync<T>(HttpClient http, Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            var response = await send();
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>();
                return value is not null ? ApiResult<T>.Ok(value) : ApiResult<T>.Fail("The server returned an empty response.");
            }

            var error = await TryReadErrorAsync(response);
            return ApiResult<T>.Fail(error?.Error ?? "Request failed.");
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail(UnreachableMessage);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Fail("The request timed out. Try again.");
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail("The server returned an unexpected response.");
        }
    }

    public static async Task<ApiResult<bool>> PostStatusAsync<TBody>(HttpClient http, string url, TBody body) =>
        await SendForStatusAsync(http, () => http.PostAsJsonAsync(url, body));

    public static async Task<ApiResult<bool>> PostStatusAsync(HttpClient http, string url) =>
        await SendForStatusAsync(http, () => http.PostAsync(url, null));

    public static async Task<ApiResult<bool>> DeleteStatusAsync(HttpClient http, string url) =>
        await SendForStatusAsync(http, () => http.DeleteAsync(url));

    public static async Task<T?> GetNullableAsync<T>(HttpClient http, string url) where T : class
    {
        try
        {
            var response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<List<T>> GetListAsync<T>(HttpClient http, string url)
    {
        try
        {
            return await http.GetFromJsonAsync<List<T>>(url) ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static async Task<PagedResult<T>> GetPagedAsync<T>(HttpClient http, string url) =>
        await GetNullableAsync<PagedResult<T>>(http, url) ?? new PagedResult<T>();

    private static async Task<ApiResult<bool>> SendForStatusAsync(HttpClient http, Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            var response = await send();
            if (response.IsSuccessStatusCode)
            {
                return ApiResult<bool>.Ok(true);
            }

            var error = await TryReadErrorAsync(response);
            return ApiResult<bool>.Fail(error?.Error ?? "Request failed.");
        }
        catch (HttpRequestException)
        {
            return ApiResult<bool>.Fail(UnreachableMessage);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<bool>.Fail("The request timed out. Try again.");
        }
        catch (JsonException)
        {
            return ApiResult<bool>.Fail("The server returned an unexpected response.");
        }
    }

    private static async Task<ApiErrorResponse?> TryReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}