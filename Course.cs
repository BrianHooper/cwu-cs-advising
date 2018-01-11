using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSAdvising
{
    class Course
    {
        String name, title, description;
        List<Course> prereqs;
        List<Quarter> quartersOffered;
        GradReq usedForReq;

        bool prereqsMet(Student s)
        {
            return false;
        }
    }
}
