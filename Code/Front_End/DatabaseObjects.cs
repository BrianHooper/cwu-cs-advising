using CwuAdvising.Models;
using Database_Object_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising
{
    /// <summary>Class for storing frequently accessed database objects in memory</summary>
    public class DatabaseObjects
    {
        /// <summary>
        /// Master list of courses contained in database
        /// </summary>
        public List<Course> MasterCourseList = new List<Course>();

        /// <summary>
        /// Retrieves master list of courses from database
        /// </summary>
        public List<Course> GetCoursesFromDatabase(bool shallow)
        {
            List<Course> CourseList = new List<Course>();
            if (Program.Database.connected)
            {
                DatabaseInterface.WriteToLog("Attempting to load all courses from database.");
                CourseList = Program.Database.GetAllCourses(shallow);
            }
            else
            {
                // Temporary values for testing
                Course MATH330 = new Course("Discrete Math", "MATH330", 5, false, new bool[] { false, true, false, true })
                {
                    Department = "Mathematics"
                };
                CourseList.Add(MATH330);

                Course CS301 = new Course("Data Structures I", "CS301", 4, false, new bool[] { true, true, false, true })
                {
                    Department = "Computer Science"
                };
                CourseList.Add(CS301);

                Course CS302 = new Course("Data Structures II", "CS302", 4, false, new bool[] { true, true, false, true })
                {
                    Department = "Computer Science"
                };
                CS302.AddPreRequisite(CS301);
                CourseList.Add(CS302);

                Course CS470 = new Course("Operating Systems", "CS470", 4, false, new bool[] { true, true, false, true })
                {
                    Department = "Computer Science"
                };
                CS470.AddPreRequisite(new Course("Prog. Language Design", "CS362", 4, false));
                CourseList.Add(CS470);
            }
            MasterCourseList = CourseList;
            return CourseList;
        }
    }
}
