using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSCHS.Models
{
    public class Assignment
    {
        public int ID { get; set; }
        public int IncidentID { get; set; }
        public int EmployeeID { get; set; }
        public int TransportID { get; set; }
        public DateTime Date { get; set; }
    }
}
