using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising.Models
{
    /// <summary>Model for Course Management</summary>
    public class CourseModel
    {
        /// <summary></summary>
        public string Name { get; set; }
        /// <summary></summary>
        public string Department { get; set; }
        /// <summary></summary>
        public string ID { get; set; }
        /// <summary></summary>
        public string Credits { get; set; }
        /// <summary></summary>
        public string Offered { get; set; }
        /// <summary></summary>
        public bool RequiresMajor { get; set; }
        /// <summary></summary>
        public List<String> PreReqs { get; set; } = new List<String>();
        /// <summary>If the course should be deleted</summary>
        public bool Delete { get; set; } = false;

        /// <summary>Explicit cast operator for CourseModel to Course conversion.</summary>
        /// <returns>A Course object equivalent to the given CourseModel.</returns>
        public static explicit operator Course(CourseModel model)
        {
            List<Course> preRequs = new List<Course>();

            bool[] offered = new bool[4];

            for (int i = 0; i < model.Offered.Length; i++)
            {
                switch(model.Offered[i])
                {
                    case '1':
                        offered[0] = true;
                        break;
                    case '2':
                        offered[1] = true;
                        break;
                    case '3':
                        offered[2] = true;
                        break;
                    case '4':
                        offered[3] = true;
                        break;
                } // end switch
            } // end for

            if (model.Credits == null)
            {
                model.Credits = "0";
            }
            Course course = new Course("", model.ID, uint.Parse(model.Credits), model.RequiresMajor, offered, preRequs);
            if (model.Name != null)
            {
                course.Name = model.Name;
            }
            else
            {
                course.Name = "";
            }

            if (model.Department != null)
            {
                course.Department = model.Department;
            }
            else
            {
                course.Department = "no department";
            }
            return course;
        } // end explicit cast operator

        /// <summary>Explicit cast operator for Course to CourseModel conversion.</summary>
        /// <returns>A CourseModel equivalent to the given Course.</returns>
        /// <param name="course">The course to be converted into a CourseModel.</param>
        public static CourseModel Convert(Course course)
        {
            CourseModel model = new CourseModel();

            model.Credits = course.Credits.ToString();
            model.Department = course.Department;

            string offered = string.Empty;

            if (course.IsOffered(Season.Winter))
            {
                offered = "1";
            } // end if
            if (course.IsOffered(Season.Spring))
            {
                offered += "2";
            } // end if
            if (course.IsOffered(Season.Summer))
            {
                offered += "3";
            } // end if
            if (course.IsOffered(Season.Fall))
            {
                offered += "4";
            } // end if

            model.Offered = offered;
            model.Name = course.Name;
            model.ID = course.ID;

            DatabaseInterface.WriteToLog("Converting Course to CourseModel: " + course.ID + " " + model.ID + " " + course.Department + " " + model.Department);
            return model;
        }// end explicit cast operator
    }
}
