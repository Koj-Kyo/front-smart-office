namespace MySmartApp.Ui.Services;

public class ThemeService : IThemeService
{
    private readonly ILocalStorageService _localStorage;
    private bool _isDarkMode = true;

    public bool IsDarkMode => _isDarkMode;
    public event Action? OnThemeChanged;

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _ = LoadThemeAsync(); // Fire and forget
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await _localStorage.SetItemAsync("isDarkMode", _isDarkMode.ToString());
        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(bool isDarkMode)
    {
        if (_isDarkMode != isDarkMode)
        {
            _isDarkMode = isDarkMode;
            await _localStorage.SetItemAsync("isDarkMode", _isDarkMode.ToString());
            OnThemeChanged?.Invoke();
        }
    }

    private async Task LoadThemeAsync()
    {
        try
        {
            var saved = await _localStorage.GetItemAsync("isDarkMode");
            if (bool.TryParse(saved, out var savedTheme))
            {
                _isDarkMode = savedTheme;
                OnThemeChanged?.Invoke();
            }
        }
        catch
        {
            // Utiliser la valeur par défaut
        }
    }
}