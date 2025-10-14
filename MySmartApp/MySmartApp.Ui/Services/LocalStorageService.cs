using Microsoft.JSInterop;

namespace MySmartApp.Ui.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Erreur LocalStorage.SetItem: {ex.Message}");
            // Ignorer l'erreur pour éviter de casser l'application
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LocalStorage.SetItem: {ex.Message}");
        }
    }

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Erreur LocalStorage.GetItem: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LocalStorage.GetItem: {ex.Message}");
            return null;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSException ex)
        {
            Console.WriteLine($"Erreur LocalStorage.RemoveItem: {ex.Message}");
            // Ignorer l'erreur pour éviter de casser l'application
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LocalStorage.RemoveItem: {ex.Message}");
        }
    }
}