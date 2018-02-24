using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CwuAdvising.Pages
{
    /// <summary>Model for student advising</summary>
    public class AdvisingModel : PageModel
    {
        /// <summary>Current student loaded to advising page</summary>
        public static StudentModel CurrentStudent { get; set; }

        /// <summary>Model for passing Student information from the view</summary>
        public class StudentModel
        {
            /// <summary>Constructor for StudentModel</summary>
            /// <param name="name">Name of the student</param>
            /// <param name="id">ID of the student</param>
            /// <param name="quarter">Student's Starting Quarter</param>
            /// <param name="year">Student's Starting Year</param>
            /// <param name="degree">Student's Degree</param>
            public StudentModel(string name, string id, string quarter, string year, string degree)
            {
                this.Name = name;
                this.ID = id;
                this.Quarter = quarter;
                this.Year = year;
                this.Degree = degree;
            }

            /// <summary>Name of current student loaded in advising page</summary>
            public string Name { get; set; } = "NoName";

            /// <summary>ID of current student loaded in advising page</summary>
            public string ID { get; set; } = "NoID";

            /// <summary>Quarter of current student loaded in advising page</summary>
            public string Quarter { get; set; } = "NoQuarter";

            /// <summary>Year of current student loaded in advising page</summary>
            public string Year { get; set; } = "NoYear";

            /// <summary>Degree of current student loaded in advising page</summary>
            public string Degree { get; set; } = "NoDegree";
        }
        
        /// <summary>Attempts to load a student plan from the database</summary>
        /// <param name="ID">ID of student</param>
        /// <returns>true if the student was found in the database</returns>
        public static bool LoadStudent(string ID)
        {
            // Find student from database that matches ID
            // Return true or false depending on whether or not the student exists

            // For testing 
            CurrentStudent = new StudentModel("Example", ID, "Fall", "2018", "BS - Computer Science");
            return true;
        }

        /// <summary>Create a new student</summary>
        /// <param name="Student">StudentModel containing student information</param>
        public static void CreateStudent(StudentModel Student)
        {
            CurrentStudent = Student;
        }
    }
}