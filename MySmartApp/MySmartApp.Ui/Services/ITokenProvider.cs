using System.Threading.Tasks;

namespace MySmartApp.Ui.Services;

public interface ITokenProvider
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
}