using System.Threading.Tasks;

namespace MySmartApp.Ui.Services;

public interface ILocalStorageService
{
    Task SetItemAsync(string key, string value);
    Task<string?> GetItemAsync(string key);
    Task RemoveItemAsync(string key);
}