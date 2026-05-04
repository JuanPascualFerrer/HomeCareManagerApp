using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class Availability
    {
        public string AvailabilityId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Zone { get; set; } = string.Empty;

        // Foreign key
        public string UserId { get; set; } = string.Empty;
    }
}
