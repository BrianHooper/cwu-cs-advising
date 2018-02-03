using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database_Object_Classes;

namespace CwuAdvising
{
    public class DemoSchedule
    {
        public string Quarter;
        public List<String> course;
        public DemoSchedule next;

        public DemoSchedule(string qtr)
        {
            this.Quarter = qtr;
            course = new List<string>();
        }

        public Boolean hasNext()
        {
            if(next == null)
            {
                return false;
            } else
            {
                return true;
            }
        }
    }
}
