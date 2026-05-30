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
        public int AssignmentCount { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string CurrentUserAssignmentId { get; set; } = string.Empty;
        public string CurrentUserAssignmentStatusId { get; set; } = string.Empty;
        public string CurrentUserAssignmentStatusName { get; set; } = string.Empty;
    }

    public class IncidentSummary
    {
        public string IncidentId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class ReportSummary
    {
        public string ReportId { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string StatusBefore { get; set; } = string.Empty;
        public string StatusAfter { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string SkillId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
