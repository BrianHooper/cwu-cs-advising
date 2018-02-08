using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Quarter quarterName = new Quarter(2018, Season.Fall);
        public List<Course> courses;
        public Schedule NextQuarter, previousQuarter;

        public Schedule(Quarter quarter, uint ui_numberCredits)
        {
            ui_numberCredits = 0;
            quarter = new Quarter(2018, Season.Fall);
            courses = new List<Course>();
        } // end Constructor

        //add course to the list
        public List<Course> addClass(Course c)
        {
            courses.Add(c);
            return courses;
        }

        public Schedule nextQuarter()
        {
            if (NextQuarter == null)
            {
                NextQuarter = new Schedule(quarterName, 0);
                NextQuarter.previousQuarter = this;
            }
            return NextQuarter;
        }

        //remove course from list
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

