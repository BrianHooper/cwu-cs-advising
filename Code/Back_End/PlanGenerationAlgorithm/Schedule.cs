using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Quarter quarterName = new Quarter(2018, Season.Fall);
        private static uint ui_numberCredits = 0;
        public List<Course> courses;
        public Schedule NextQuarter, previousQuarter;


        public Schedule(Quarter quarter)
        {
            this.quarterName = quarter;
            courses = new List<Course>();
        } // end Constructor


        public List<Course> addClass(Course c) {
            courses.Add(c);
            return courses;
        }
        public Schedule nextQuarter()
        {

            if (NextQuarter == null)
            {
                NextQuarter = new Schedule(GetNextQuarter());
                NextQuarter.previousQuarter = this;
            }

            return NextQuarter;
            
        }

        private Quarter GetNextQuarter()
        {
            switch (quarterName.QuarterSeason)
            {
                case Season.Fall: return new Quarter(quarterName.Year + 1, Season.Winter);
                case Season.Winter: return new Quarter(quarterName.Year, Season.Spring);
                case Season.Spring: return new Quarter(quarterName.Year, Season.Fall);
                default: return quarterName;
            }
        }


        public List<Course> removeClass(Course c)
        {
            courses.Remove(c);
            return courses;
        }


        public override string ToString()
        {
            String outputStr = "";
            Schedule ScheduleIterator = this;

            while (ScheduleIterator != null)
            {
                outputStr += ScheduleIterator.quarterName + "\n";

                if (ScheduleIterator.courses.Count == 0)
                {
                    outputStr += "---EMPTY--\n";
                }

                else
                {
                    foreach (Course c in ScheduleIterator.courses)
                    {
                        outputStr += "Course: " + c.ID + "\n";
                    }
                }
                ScheduleIterator = ScheduleIterator.NextQuarter;
            }

            return outputStr;
        }

    }
}
