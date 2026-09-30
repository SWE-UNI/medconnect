using Microsoft.JSInterop;

namespace MedConnect.Client.Services;

public class ThemeService(IJSRuntime js)
{
    private bool _isDarkMode;
    private bool _initialized;

    public bool IsDarkMode => _isDarkMode;

    public event Action? OnThemeChanged;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var saved = await js.InvokeAsync<string?>("localStorage.getItem", "medconnect_theme");
            if (saved == "dark")
            {
                _isDarkMode = true;
            }
            else if (saved == "light")
            {
                _isDarkMode = false;
            }
            else
            {
                _isDarkMode = await js.InvokeAsync<bool>("eval", "document.documentElement.classList.contains('dark') || (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches)");
            }

            await ApplyThemeAsync(_isDarkMode);
        }
        catch
        {
            // Prerender or storage unavailable
        }
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyThemeAsync(_isDarkMode);
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", "medconnect_theme", _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore storage errors
        }

        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(bool isDark)
    {
        if (_isDarkMode == isDark && _initialized) return;
        _isDarkMode = isDark;
        await ApplyThemeAsync(_isDarkMode);
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", "medconnect_theme", _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore storage errors
        }

        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(bool isDark)
    {
        try
        {
            if (isDark)
            {
                await js.InvokeVoidAsync("eval", "document.documentElement.classList.add('dark')");
            }
            else
            {
                await js.InvokeVoidAsync("eval", "document.documentElement.classList.remove('dark')");
            }
        }
        catch
        {
            // Ignore JS errors
        }
    }
}
