using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class TaskItem
    {
        public string TaskId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Priority { get; set; } = string.Empty;

        // Foreign keys
        public string RequiredSkillId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string StatusId { get; set; } = string.Empty;
    }
}
