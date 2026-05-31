using System;

namespace HomeCareManager.Core.Models
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool PasswordChanged { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string SkillId { get; set; } = string.Empty;
    }
}
