namespace MySmartApp.Ui.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event Action? OnThemeChanged;
    Task ToggleThemeAsync();
    Task SetThemeAsync(bool isDarkMode);
}