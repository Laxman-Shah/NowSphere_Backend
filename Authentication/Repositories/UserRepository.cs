using Microsoft.EntityFrameworkCore;
using smartApi.Authentication.Repositories.Interface;
using smartApi.Data;
using smartApi.Entity;

namespace smartApi.Authentication.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // EXISTING USER OPERATIONS
        // =====================================================

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }


        public async Task<User?> GetByIdAsync(long id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id);
        }


        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }


        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Username == username);
        }


        public async Task<User?> GetByEmailWithCredentialAsync(
            string email)
        {
            return await _context.Users
                .Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.Email == email);
        }


        public async Task<User?> GetByUsernameWithCredentialAsync(
            string username)
        {
            return await _context.Users
                .Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.Username == username);
        }


        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .ToListAsync();
        }


        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(x => x.Email == email);
        }


        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users
                .AnyAsync(x => x.Username == username);
        }


        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(x => x.RoleName == roleName);
        }


        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }


        // =====================================================
        // USER DEVICE OPERATIONS
        // =====================================================

        public async Task<UserDevice?> GetUserDeviceByFingerprintAsync(
            long userId,
            string fingerprintHash)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DeviceFingerprintHash == fingerprintHash);
        }


        public async Task<UserDevice> AddUserDeviceAsync(
            UserDevice userDevice)
        {
            await _context.UserDevices.AddAsync(userDevice);
            await _context.SaveChangesAsync();

            return userDevice;
        }


        // =====================================================
        // USER SESSION OPERATIONS
        // =====================================================

        public async Task<UserSession> AddUserSessionAsync(
            UserSession userSession)
        {
            await _context.UserSessions.AddAsync(userSession);
            await _context.SaveChangesAsync();

            return userSession;
        }


        public async Task<UserSession?> GetOwnedSessionWithTokensAsync(
            long userId,
            Guid sessionId)
        {
            return await _context.UserSessions
                .Include(x => x.UserDevice)
                .Include(x => x.RefreshTokens)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.UserSessionId == sessionId);
        }


        public async Task<List<UserSession>> GetUserSessionsAsync(
            long userId)
        {
            return await _context.UserSessions
                .AsNoTracking()
                .Include(x => x.UserDevice)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LastActivityAt)
                .ToListAsync();
        }


        public async Task<List<UserSession>> GetOtherActiveSessionsAsync(
            long userId,
            Guid currentSessionId)
        {
            return await _context.UserSessions
                .Include(x => x.UserDevice)
                .Include(x => x.RefreshTokens)
                .Where(x =>
                    x.UserId == userId &&
                    x.UserSessionId != currentSessionId &&
                    x.Status == "ACTIVE" &&
                    x.RevokedAt == null &&
                    x.LoggedOutAt == null)
                .ToListAsync();
        }


        public async Task<List<UserSession>> GetAllActiveSessionsAsync(
            long userId)
        {
            return await _context.UserSessions
                .Include(x => x.UserDevice)
                .Include(x => x.RefreshTokens)
                .Where(x =>
                    x.UserId == userId &&
                    x.Status == "ACTIVE" &&
                    x.RevokedAt == null &&
                    x.LoggedOutAt == null)
                .ToListAsync();
        }


        // =====================================================
        // LOGIN ACTIVITY OPERATIONS
        // =====================================================

        public async Task<LoginActivity> AddLoginActivityAsync(
            LoginActivity loginActivity)
        {
            await _context.LoginActivities.AddAsync(loginActivity);
            await _context.SaveChangesAsync();

            return loginActivity;
        }


        public async Task<(List<LoginActivity> Items, int TotalCount)>
            GetUserLoginActivitiesAsync(
                long userId,
                int page,
                int pageSize,
                string? eventType,
                string? outcome)
        {
            IQueryable<LoginActivity> query =
                _context.LoginActivities
                    .AsNoTracking()
                    .Where(x => x.UserId == userId);

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(
                    x => x.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(outcome))
            {
                query = query.Where(
                    x => x.Outcome == outcome);
            }

            int totalCount = await query.CountAsync();

            List<LoginActivity> items = await query
                .OrderByDescending(x => x.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }


        // =====================================================
        // PROFILE-SPECIFIC OPERATIONS
        // =====================================================

        public async Task<User?> GetUserWithActiveRolesAsync(long userId)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<User?> GetUserWithCredentialAsync(long userId)
        {
            return await _context.Users
                .Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> CountActiveSessionsAsync(long userId)
        {
            return await _context.UserSessions
                .CountAsync(x =>
                    x.UserId == userId &&
                    x.Status == "ACTIVE" &&
                    x.RevokedAt == null &&
                    x.LoggedOutAt == null &&
                    x.ExpiresAt > DateTime.UtcNow);
        }
    }
}