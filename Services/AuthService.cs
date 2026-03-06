using QRRewardPlatform.Models;

namespace QRRewardPlatform.Services
{
    public class AuthService
    {
        private readonly FirebaseService _firebase;

        public AuthService(FirebaseService firebase)
        {
            _firebase = firebase;
        }

        public async Task<AdminUser?> ValidateLoginAsync(string username, string password)
        {
            var admins = await _firebase.GetAllAsync<AdminUser>("admins");
            var admin = admins.Values.FirstOrDefault(a => 
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (admin == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
                return null;

            // Update last login
            admin.LastLogin = DateTime.UtcNow.ToString("o");
            var entry = admins.First(a => a.Value.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            admin.Id = entry.Key;
            await _firebase.SetAsync("admins", entry.Key, admin);

            return admin;
        }

        public async Task CreateAdminAsync(string username, string password, string displayName, string role = "Admin")
        {
            var admin = new AdminUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                DisplayName = displayName,
                Role = role,
                CreatedAt = DateTime.UtcNow.ToString("o"),
                LastLogin = ""
            };

            await _firebase.PushAsync("admins", admin);
        }

        public async Task SeedDefaultAdminAsync()
        {
            var admins = await _firebase.GetAllAsync<AdminUser>("admins");
            
            // Remove old default admin if it exists to strictly follow the new default prompt
            var oldAdmin = admins.FirstOrDefault(a => a.Value.Username == "admin");
            if (oldAdmin.Key != null)
            {
                await _firebase.DeleteNodeAsync($"admins/{oldAdmin.Key}");
            }

            if (!admins.Values.Any(a => a.Username == "dhajare94"))
            {
                await CreateAdminAsync("dhajare94", "Sbr00216@123", "Administrator", "Admin");
            }
        }

        public async Task<List<AdminUser>> GetAllAdminsAsync()
        {
            var admins = await _firebase.GetAllAsync<AdminUser>("admins");
            return admins.Select(a => { a.Value.Id = a.Key; return a.Value; }).ToList();
        }

        public async Task DeleteAdminAsync(string id)
        {
            await _firebase.DeleteAsync("admins", id);
        }
    }
}
