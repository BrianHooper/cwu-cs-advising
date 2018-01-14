using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSAdvising
{
    class QuarterSchedule
    {
        Quarter quarter;
        int totalCredits;
        List<Course> coursesTaken;
        QuarterSchedule s;

        QuarterSchedule getNextQtr()
        {
            return s;
        }

        bool addCourse()
        {
            return false;
        }

    }
}
