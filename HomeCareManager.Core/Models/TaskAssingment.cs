using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class TaskAssingment
    {
        public string AssignmentId { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string StatusId { get; set; } = string.Empty;
    }
}
