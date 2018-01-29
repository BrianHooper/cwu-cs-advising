using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    class Schedule
    {
        HashSet<Course> h = new HashSet<Course>();
        public Schedule(Quarter quarter, uint ui_numberCredits,List<Course> courses)
        {
            this.quarter = quarter;

            this.ui_numberCredits = ui_numberCredits;
            this.courses = new List<Course>();
            this.NextQuarter = nextquarter1;
        } // end Constructor

        Quarter quarter;
        private uint ui_numberCredits;
        int credits;
        List<Course> courses;
        private Schedule NextQuarter;
        Schedule nextquarter1;
    }
}
