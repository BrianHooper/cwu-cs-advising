using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        //variables
        public Student student;
        public Quarter quarterName;
        public bool locked = false;
        public uint NumberOfQuarters; //total number of quarters
        public List<Course> courses; //list of all courses taken
        public Schedule NextQuarter, previousQuarter;
        public uint ui_numberCredits=0; 
        public bool TakeSummerCourses = false;

        //constructor
        public Schedule(Quarter quarter)
        {
            quarterName = quarter;
            courses = new List<Course>();
        } // end Constructor

        //method to check if course meets constraints or not
        public bool MeetsConstraints(Course c)
        {
            return (ui_numberCredits < 16 && !courses.Contains(c));
        }

        //method to get the lower bound to check the best possible solution
        public uint lowerBound()
        {
            uint totalRemainingCredits = 0;
            foreach (Course c in courses)
            {
                totalRemainingCredits += c.Credits;
            }
            return NumberOfQuarters + (totalRemainingCredits / Algorithm.maxCredits);
        }

        //method to add course to the list
        public bool AddCourse(Course c)
        {
            if (MeetsConstraints(c))
            {
                courses.Add(c);
                ui_numberCredits += c.Credits; //add current number of credits with course c
                return true;
            }
            else
            {
                return false;
            }

        }


        //method to get the schedule for next quarter
        public Schedule NextSchedule()
        {
            
            if (NextQuarter == null)
            {
                NextQuarter = new Schedule(GetNextQuarter());
                this.NumberOfQuarters++;
                NextQuarter.previousQuarter = this;
            }
            if (NextQuarter.locked)
            {
                return NextQuarter.NextSchedule();
            }
            return NextQuarter;
        }

        //method to increment the quarter season and/or year
        private Quarter GetNextQuarter()
        {
            switch (quarterName.QuarterSeason)
            {
                case Season.Fall:  return new Quarter(quarterName.Year + 1, Season.Winter);
                case Season.Winter:  return new Quarter(quarterName.Year, Season.Spring);
                case Season.Spring:
                    {
                        if (TakeSummerCourses)
                        {
                            //NumberOfQuarters++;
                            return new Quarter(quarterName.Year, Season.Summer);
                        }
                        else
                        {
                           //NumberOfQuarters++;
                            return new Quarter(quarterName.Year, Season.Fall);
                        }
                    }
                default: return quarterName;
            }
        }

        //method to remove course from list
        public List<Course> RemoveCourse(Course c)
        {
            courses.Remove(c);
            return courses;
        }

        //toString method override to print all the schedules
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

        //method to check schedule for previous quarter
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

