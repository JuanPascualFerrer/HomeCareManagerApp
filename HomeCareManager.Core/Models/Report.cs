using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class Report
    {
        public string ReportId { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string StatusBefore { get; set; } = string.Empty;
        public string StatusAfter { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
    }
}
