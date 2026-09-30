using Microsoft.JSInterop;

namespace MedConnect.Client.Services;

public class TokenStorage(IJSRuntime js)
{
    private const string Key = "medconnect_token";

    public ValueTask<string?> GetTokenAsync() => js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SetTokenAsync(string token) => js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public ValueTask ClearTokenAsync() => js.InvokeVoidAsync("localStorage.removeItem", Key);
}
