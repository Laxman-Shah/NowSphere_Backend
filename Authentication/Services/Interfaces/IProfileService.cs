using smartApi.Authentication.DTOs.Profile.Requests;
using smartApi.Authentication.DTOs.Profile.Responses;

namespace smartApi.Authentication.Services.Interfaces;

public interface IProfileService
{
    // ============================================================
    // GET PROFILE
    // ============================================================

    Task<MyProfileResponseDto> GetMyProfileAsync(
        CancellationToken cancellationToken = default);

    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    Task<UpdateProfileResponseDto> UpdateMyProfileAsync(
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default);

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    Task<ChangePasswordResponseDto> ChangeMyPasswordAsync(
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default);
}
