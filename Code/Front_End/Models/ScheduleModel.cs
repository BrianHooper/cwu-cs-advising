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
