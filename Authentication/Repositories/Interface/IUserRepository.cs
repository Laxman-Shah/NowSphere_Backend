using smartApi.Entity;

namespace smartApi.Authentication.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);

        Task<User?> GetByIdAsync(long id);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByUsernameAsync(string username);

        Task<User?> GetByEmailWithCredentialAsync(string email);

        Task<List<User>> GetAllAsync();

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByUsernameAsync(string username);

        Task<Role?> GetRoleByNameAsync(string roleName);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);

        Task<User?> GetByUsernameWithCredentialAsync(string username);




            // =====================================================
            // USER DEVICE OPERATIONS
            // =====================================================

            Task<UserDevice?> GetUserDeviceByFingerprintAsync(
                long userId,
                string fingerprintHash);

            Task<UserDevice> AddUserDeviceAsync(
                UserDevice userDevice);


            // =====================================================
            // USER SESSION OPERATIONS
            // =====================================================

            Task<UserSession> AddUserSessionAsync(
                UserSession userSession);

            Task<UserSession?> GetOwnedSessionWithTokensAsync(
                long userId,
                Guid sessionId);

            Task<List<UserSession>> GetUserSessionsAsync(
                long userId);

            Task<List<UserSession>> GetOtherActiveSessionsAsync(
                long userId,
                Guid currentSessionId);

            Task<List<UserSession>> GetAllActiveSessionsAsync(
                long userId);


            // =====================================================
            // LOGIN ACTIVITY OPERATIONS
            // =====================================================

            Task<LoginActivity> AddLoginActivityAsync(
                LoginActivity loginActivity);

            Task<(List<LoginActivity> Items, int TotalCount)>
                GetUserLoginActivitiesAsync(
                    long userId,
                    int page,
                    int pageSize,
                    string? eventType,
                    string? outcome);

        // =====================================================
        // PROFILE-SPECIFIC OPERATIONS
        // =====================================================

        Task<User?> GetUserWithActiveRolesAsync(long userId);

        Task<User?> GetUserWithCredentialAsync(long userId);

        Task<int> CountActiveSessionsAsync(long userId);
    }
}
