using System;
using System.Collections.Generic;
using System.Text;

using HomeCareManager.Core.Data;

using HomeCareManager.Core.Models;

namespace HomeCareManager.Core.Services
{
    public class AuthService
    {
        private readonly HomeCareManager.Core.Data.Data data = new HomeCareManager.Core.Data.Data();

        public User? Login(string email, string password)
        {
            // 1. Buscar usuario por email
            // 2. Comprobar si existe
            // 3. Comprobar si esta activo
            // 4. Comprobar password
            // 5. Devolver usuario si todo esta bien
            return null;
        }

    }
}
