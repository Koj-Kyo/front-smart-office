using System.Net.Http;
using System.Net.Http.Json;
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            return await client.GetFromJsonAsync<UserProfileDto>("/User/profile");
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(UserProfileDto profile)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PutAsJsonAsync("/User/profile", profile);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserProfileDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AccessRightDto>> GetUserZoneAccessAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var access = await client.GetFromJsonAsync<List<AccessRightDto>>("/user/access-rights/zones");
            return access ?? new List<AccessRightDto>();
        }
        catch
        {
            return new List<AccessRightDto>();
        }
    }

    public async Task<List<AccessRightDto>> GetUserDeviceAccessAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var access = await client.GetFromJsonAsync<List<AccessRightDto>>("/user/access-rights/peripheriques");
            return access ?? new List<AccessRightDto>();
        }
        catch
        {
            return new List<AccessRightDto>();
        }
    }
}