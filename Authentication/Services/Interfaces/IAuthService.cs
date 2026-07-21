/*using smartApi.DTOs.Auth.Requests;
using smartApi.DTOs.Auth.Responses; // Make sure this matches your folder name (plural vs singular)
using System.Threading.Tasks;

namespace smartApi.Services.Interface
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}
*/



/* public interface IAuthService
     {

         Task<RegisterResponseDto> RegisterAsync(
             RegisterRequestDto request,
             string? ipAddress,
             string? userAgent
         );

         Task<VerifyRegisterOtpResponseDto> VerifyRegisterOtpAsync(
             VerifyRegisterOtpRequestDto request
         );

         Task<LoginStep1ResponseDto> LoginAsync(
             LoginRequestDto request,
             string? ipAddress,
             string? userAgent
         );

         Task<LoginResponseDto> RefreshTokenAsync(
     string refreshToken,
     string? ipAddress,
     string? userAgent
 );



         Task<LogoutResponseDto> RevokeTokenAsync(
                     string rawRefreshToken,
                     string userId,
                     string? ipAddress
                 );




     }





    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(
            RegisterRequestDto request,
            string? ipAddress,
            string? userAgent
        );

        Task<VerifyRegisterOtpResponseDto>
            VerifyRegisterOtpAsync(
                VerifyRegisterOtpRequestDto request
            );

        Task<LoginStep1ResponseDto> LoginAsync(
            LoginRequestDto request,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default
        );

        Task<LoginResponseDto> VerifyLoginOtpAsync(
            VerifyLoginOtpRequestDto request,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default
        );

        Task<ResendLoginOtpResponseDto>
            ResendLoginOtpAsync(
                ResendLoginOtpRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default
            );

        Task<LoginResponseDto> RefreshTokenAsync(
            string refreshToken,
            string? ipAddress,
            string? userAgent
        );

        Task<LogoutResponseDto> RevokeTokenAsync(
            string rawRefreshToken,
            string userId,
            string? ipAddress
        );
    }



}

    */


using smartApi.Authentication.DTOs.Auth.Requests;
using smartApi.Authentication.DTOs.Auth.Responses;

namespace smartApi.Authentication.Services.Interfaces
{


        public interface IAuthService
        {
            // ============================================================
            // REGISTRATION
            // ============================================================

            Task<RegisterResponseDto> RegisterAsync(
                RegisterRequestDto request,
                string? ipAddress,
                string? userAgent
            );

            Task<VerifyRegisterOtpResponseDto>
                VerifyRegisterOtpAsync(
                    VerifyRegisterOtpRequestDto request
                );

            // ============================================================
            // LOGIN STEP 1
            // PASSWORD VERIFICATION + LOGIN OTP CHALLENGE
            // ============================================================

            Task<LoginStep1ResponseDto> LoginAsync(
                LoginRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default
            );

            // ============================================================
            // LOGIN STEP 2
            // LOGIN OTP VERIFICATION + TOKEN ISSUANCE
            // ============================================================

            Task<LoginResponseDto> VerifyLoginOtpAsync(
                VerifyLoginOtpRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default
            );

            // ============================================================
            // RESEND LOGIN OTP
            // ============================================================

            Task<ResendLoginOtpResponseDto>
                ResendLoginOtpAsync(
                    ResendLoginOtpRequestDto request,
                    string? ipAddress,
                    string? userAgent,
                    CancellationToken cancellationToken = default
                );

            // ============================================================
            // REFRESH TOKEN
            // ============================================================

            Task<LoginResponseDto> RefreshTokenAsync(
                string refreshToken,
                string? ipAddress,
                string? userAgent
            );

            // ============================================================
            // LOGOUT
            // ============================================================

            Task<LogoutResponseDto> RevokeTokenAsync(
                string rawRefreshToken,
                string userId,
                string? ipAddress
            );


        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(
        ForgotPasswordRequestDto request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

        Task<ResetPasswordResponseDto> ResetPasswordAsync(
            ResetPasswordRequestDto request,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default);








            // =====================================================
            // KEEP ALL EXISTING AUTHENTICATION METHODS
            // =====================================================

            // Examples only:
            //
            // Task<RegisterResponseDto> RegisterAsync(
            //     RegisterRequestDto request);
            //
            // Task<LoginStep1ResponseDto> LoginAsync(
            //     LoginRequestDto request);
            //
            // Task<LoginResponseDto> VerifyLoginOtpAsync(
            //     VerifyLoginOtpRequestDto request);
            //
            // Task<RefreshTokenResponseDto> RefreshTokenAsync(
            //     RefreshTokenRequestDto request);
            //
            // Task<LogoutResponseDto> LogoutAsync(
            //     LogoutRequestDto request);


            // =====================================================
            // NEW USER-SESSION METHODS
            // =====================================================

            Task<IReadOnlyCollection<UserSessionResponse>>
                GetMySessionsAsync(
                    CancellationToken cancellationToken = default);

            Task RevokeSessionAsync(
                Guid sessionId,
                string? reason,
                CancellationToken cancellationToken = default);

            Task RevokeOtherSessionsAsync(
                CancellationToken cancellationToken = default);

            Task RevokeAllSessionsAsync(
                CancellationToken cancellationToken = default);

            Task LogoutCurrentSessionAsync(
                CancellationToken cancellationToken = default);


            // =====================================================
            // NEW LOGIN-ACTIVITY METHOD
            // =====================================================

            Task<PagedLoginActivityResponseDto>
                GetMyLoginActivitiesAsync(
                    LoginActivityQueryRequestDto request,
                    CancellationToken cancellationToken = default);
        }
}

    