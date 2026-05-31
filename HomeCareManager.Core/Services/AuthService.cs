using HomeCareManager.Core.Models;
using DataAccess = HomeCareManager.Core.Data.Data;

namespace HomeCareManager.Core.Services
{
    public class AuthService
    {
        private readonly DataAccess data = new DataAccess();

        public User? Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            User? user = data.GetUserByEmail(email.Trim());

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }
    }
}
