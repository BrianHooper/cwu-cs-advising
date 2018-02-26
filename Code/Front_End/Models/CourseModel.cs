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

            string ID = string.Empty;

            switch(model.Department)
            {
                case "Computer Science":
                    ID = "CS";
                    break;
                case "Mathematics":
                    ID = "Math";
                    break;
            } // end switch
            
            return new Course(model.Name, model.ID, uint.Parse(model.Credits), model.RequiresMajor, offered, preRequs);
        } // end explicit cast operator
    }
}
