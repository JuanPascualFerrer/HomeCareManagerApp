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

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
    }
}
