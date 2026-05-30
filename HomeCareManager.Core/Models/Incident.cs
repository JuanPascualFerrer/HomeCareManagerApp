using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class Incident
    {
        public string IncidentId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string AssignedToUserId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
    }
}
