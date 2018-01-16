using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSAdvising
{
    class Student
    {
        String name, id;
        int maxCredits, minCredits;
        List<GradReq> unmetReqs;
        HashSet<GradReq> metReqs;
        HashSet<Course> coursesTaken;
        List<QuarterSchedule> schedule;
        bool hasRemainingRequirements()
        {
            return false;
        }
        HashSet<Course> getPossibleCourses(Quarter q)
        {
            return coursesTaken;
        }
    }
}
