using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync();
    Task<UserProfileDto?> UpdateProfileAsync(UserProfileDto profile);
    Task<List<AccessRightDto>> GetUserZoneAccessAsync();
    Task<List<AccessRightDto>> GetUserDeviceAccessAsync();
}