using Database_Object_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising.Models
{
    /// <summary>
    /// Model for Advising Schedule
    /// </summary>
    public class ScheduleModel
    {
        /// <summary>
        /// Model quarter containing title, locked value, and list of course
        /// </summary>
        public class ModelQuarter
        {
            /// <summary>Quarter title in "Season Year" format</summary>
            public string Title { get; set; }

            /// <summary>True if the quarter is locked</summary>
            public bool Locked { get; set; }

            /// <summary>List of courses for the quarter</summary>
            public List<Requirement> Courses { get; set; }
        }

        /// <summary>
        /// Represents a single course
        /// </summary>
        public class Requirement
        {
            /// <summary>Title if the course</summary>
            public string Title { get; set; }

            /// <summary>Number of credits</summary>
            public string Credits { get; set; }

            /// <summary>
            /// When the course is offerd in "1234" format, 
            /// Representing "Winter Spring Summer Fall"
            /// </summary>
            public string Offered { get; set; }

            /// <summary>Converts a Course object to a Requirement object</summary>
            /// <param name="course">The Course to be converted</param>
            /// <returns>The Requirement corresponding to the given Course</returns>
            public static Requirement CourseToRequirement(Course course)
            {
                Requirement requirement = new Requirement
                {
                    Title = course.ID,
                    Credits = course.Credits.ToString(),
                    Offered = ""
                };

                if (course.IsOffered(Season.Winter))
                    requirement.Offered += "1";
                if (course.IsOffered(Season.Spring))
                    requirement.Offered += "2";
                if (course.IsOffered(Season.Summer))
                    requirement.Offered += "3";
                if (course.IsOffered(Season.Fall))
                    requirement.Offered += "4";

                return requirement;
            }
        }

        /// <summary>
        /// Contains the constraints for the schedule model
        /// </summary>
        public class ConstraintModel
        {
            /// <summary>Minimum Credits</summary>
            public uint MinCredits { get; set; }

            /// <summary>Maximim Credits</summary>
            public uint MaxCredits { get; set; }

            /// <summary>If the student can take summer courses</summary>
            public bool TakingSummer { get; set; }
        }

        /// <summary>List of Quarters</summary>
        public List<ModelQuarter> Quarters { get; set; }

        /// <summary>List of remaining requirements as Course objects</summary>
        public List<Requirement> UnmetRequirements { get; set; }

        /// <summary>Student Constraints</summary>
        public ConstraintModel Constraints;
    }
}
