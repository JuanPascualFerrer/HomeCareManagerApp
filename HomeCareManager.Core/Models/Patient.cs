using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCareManager.Core.Models
{
    public class Patient
    {
        public string PatientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
    }
}
