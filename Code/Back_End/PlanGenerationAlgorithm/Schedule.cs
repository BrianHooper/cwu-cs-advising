using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Student student;
        public Quarter quarterName;
        public bool locked = false;
        public uint NumberOfQuarters = 0;
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
            return (ui_numberCredits < 16 && !courses.Contains(c));
        }

        public uint lowerBound()
        {
            uint totalRemainingCredits = 0;
            foreach (Course c in courses)
            {
                totalRemainingCredits += c.Credits;
            }
            return NumberOfQuarters + (totalRemainingCredits / Algorithm.maxCredits);
        }
        //add course to the list
        public bool AddCourse(Course c)
        {
            if (MeetsConstraints(c))
            {
                courses.Add(c);
                ui_numberCredits += c.Credits;
                return true;
            }
            else
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
            if (NextQuarter.locked)
            {
                return NextQuarter.NextSchedule();
            }
            return NextQuarter;
        }

        private Quarter GetNextQuarter()
        {
            switch (quarterName.QuarterSeason)
            {
                case Season.Fall: NumberOfQuarters++; return new Quarter(quarterName.Year + 1, Season.Winter);
                case Season.Winter: NumberOfQuarters++; return new Quarter(quarterName.Year, Season.Spring);
                case Season.Spring:
                    {
                        if (TakeSummerCourses)
                        {
                            NumberOfQuarters++;
                            return new Quarter(quarterName.Year, Season.Summer);
                        }
                        else
                        {
                            NumberOfQuarters++;
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
            if (previousQuarter == null)
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

