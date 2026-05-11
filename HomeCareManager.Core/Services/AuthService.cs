using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


using HomeCareManager.Core.Data;

using HomeCareManager.Core.Models;

namespace HomeCareManager.Core.Services
{
    public class AuthService
    {
        private readonly HomeCareManager.Core.Data.Data data = new HomeCareManager.Core.Data.Data();

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

            if (user.PasswordHash != password)
            {
                return null;
            }

            return user;
        }


    }
}
