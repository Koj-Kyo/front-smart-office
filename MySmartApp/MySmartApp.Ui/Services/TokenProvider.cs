namespace MySmartApp.Ui.Services;

public class TokenProvider : ITokenProvider
{
    private const string TokenKey = "auth_token";
    private readonly ILocalStorageService _storage;

    public TokenProvider(ILocalStorageService storage) => _storage = storage;

    public Task<string?> GetTokenAsync() => _storage.GetItemAsync(TokenKey);
    public Task SetTokenAsync(string token) => _storage.SetItemAsync(TokenKey, token);
    public Task ClearTokenAsync() => _storage.RemoveItemAsync(TokenKey);
}