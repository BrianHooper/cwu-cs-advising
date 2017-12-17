using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Object_Classes
{
    public class Student : Database_Object
    {
        /// <summary>The first name of this student.</summary>
        private string s_fName;
        /// <summary>The last name of this student.</summary>
        private string s_lName;

        /// <summary>The quarter this student is expected to graduate.</summary>
        private Quarter q_expectedGraduation;

        /// <summary>The catalog year of this student.</summary>
        private uint   ui_catalogYear;

        /// <summary>The quarter this student started at CWU. 0 = Winter, 3 = Fall.</summary>
        private Season s_startingQuarter;

        /// <summary>Creates a new student object with the specified name, ID, and starting quarter.</summary>
        /// <param name="s_fName">First name of this student.</param>
        /// <param name="s_lName">Last name of this student.</param>
        /// <param name="s_ID">Student ID of this student.</param>
        /// <param name="q_start">Quarter in which this student enrolled.</param>
        public Student(string s_fName, string s_lName, string s_ID, Quarter q_start) : base(s_ID)
        {
            this.s_fName = s_fName;
            this.s_lName = s_lName;
            ui_catalogYear = q_start.Year;
            s_startingQuarter = q_start.QuarterSeason;

            // default graduation time
            q_expectedGraduation.QuarterSeason = Season.Spring;
            q_expectedGraduation.Year = 0;
        } // end Default Constructor
    } // end Class Student
} // end Namespace Database_Object_Classes
