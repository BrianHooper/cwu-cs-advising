using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising.Models
{
    public class CourseModel
    {
        public string Title { get; set; }
        public string Department { get; set; }
        public string Number { get; set; }
        public string Credits { get; set; }
        public string Offered { get; set; }
        public List<string> PreReqs { get; set; } = new List<string>();
    }
}
