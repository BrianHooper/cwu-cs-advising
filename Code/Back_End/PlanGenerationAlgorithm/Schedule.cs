using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Quarter quarterName;
        public List<Course> courses;
        public Schedule NextQuarter, previousQuarter;
        public uint ui_numberCredits = 0;
        public bool TakeSummerCourses = false;

        public Schedule(Quarter quarter)
        {
            quarterName = quarter;
            courses = new List<Course>();
        } // end Constructor

        public bool MeetsConstraints(Course c)
        {
            return (courses.Count < 3 && !courses.Contains(c));
        }

        //add course to the list
        public bool AddCourse(Course c)
        {
            if(MeetsConstraints(c))
            {
                courses.Add(c);
                return true;
            } else
            {
                return false;
            }
            
        }

        public Schedule NextSchedule()
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
                case Season.Spring:
                {
                        if (TakeSummerCourses)
                        {
                            return new Quarter(quarterName.Year, Season.Summer);
                        }
                        else
                        {
                            return new Quarter(quarterName.Year, Season.Fall);
                        }
                }
                default: return quarterName;
            }
        }

        //remove course from list
        public List<Course> RemoveCourse(Course c)
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
                        outputStr += "\t" + c.ID + "\n";
                    }
                }
                ScheduleIterator = ScheduleIterator.NextQuarter;
            }

            return outputStr;
        }

        public Schedule GetFirstSchedule()
        {
            if(previousQuarter == null)
            {
                return this;
            }
            else
            {
                return previousQuarter.GetFirstSchedule();
            }
        }
    }
}

