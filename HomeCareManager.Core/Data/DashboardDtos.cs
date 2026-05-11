using System;

namespace HomeCareManager.Core.Data
{
    public class TaskSummary
    {
        public string TaskId { get; set; } = string.Empty;
        public string RequiredSkillId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientZone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string StatusId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
    }

    public class IncidentSummary
    {
        public string IncidentId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
