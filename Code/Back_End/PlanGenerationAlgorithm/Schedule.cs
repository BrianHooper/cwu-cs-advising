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
        public uint NumberOfQuarters = 0; //total number of quarters
        public List<Course> courses; //list of all courses taken
        public Schedule NextQuarter, previousQuarter;
        public uint ui_numberCredits = 0;
        public bool TakeSummerCourses = false;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="quarter">current quarter</param>
        public Schedule(Quarter quarter)
        {
            quarterName = quarter;
            courses = new List<Course>();
        } // end Constructor

        /// <summary>Copy Constructor which creates a copy of the other course.</summary>
        /// <param name="allCourses">schedule to be copied.</param>
        public Schedule(Schedule allCourses) 
        {
            this.NextQuarter = allCourses.NextQuarter;
            this.previousQuarter = allCourses.previousQuarter;
            this.ui_numberCredits = allCourses.ui_numberCredits;
            this.quarterName = allCourses.quarterName;
            this.NumberOfQuarters = allCourses.NumberOfQuarters;
            this.courses = new List<Course>(allCourses.courses);

        } // end Copy Constructor
        /// <summary>
        /// method to check if course meets constraints or not
        /// </summary>
        /// <param name="c">the course to be checked</param>
        /// <returns>number of credits less than max number of credits and 
        /// if list of courses taken does not contain the course to be checked
        /// </returns>
        public bool MeetsConstraints(Course c)
        {
            return (ui_numberCredits < 18 && !courses.Contains(c));
        }

        /// <summary>
        /// method to get the lower bound to check the best possible solution
        /// </summary>
        /// <returns>lower bound</returns>
        public uint lowerBound()
        {
            uint totalRemainingCredits = 0;
            foreach (Course c in courses)
            {
                totalRemainingCredits += c.Credits;
            }
            return NumberOfQuarters + (totalRemainingCredits / Algorithm.maxCredits);
        }

        /// <summary>
        /// method to add course to the list
        /// if the course meets constraints
        /// </summary>
        /// <param name="c">possible course to be added</param>
        /// <returns>true or false depends on whether the course meet all the constraints or not</returns>
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

        /// <summary>
        /// method to get the schedule for next quarter
        /// </summary>
        /// <returns>go to the next quarter schedule</returns>
        public Schedule NextSchedule()
        {
            Algorithm algorithm = new Algorithm();
            if (NextQuarter == null)
            {
                NextQuarter = new Schedule(GetNextQuarter());
                if (NextQuarter.quarterName.QuarterSeason.Equals(Season.Summer)&&!NextQuarter.locked)
                {
                    algorithm.takeSummerCourses = true;
                }
                this.NumberOfQuarters++;
                NextQuarter.previousQuarter = this;
            }
            if (NextQuarter.locked)
            {
                return NextQuarter.NextSchedule();
            }
            return NextQuarter;
        }

        /// <summary>
        /// method to increment the quarter season and/or year
        /// </summary>
        /// <returns>new quarter with new quarter name and possible new year</returns>
        public Quarter GetNextQuarter()
        {
            Algorithm algorithm = new Algorithm();
            switch (quarterName.QuarterSeason)
            {
                case Season.Fall: return new Quarter(quarterName.Year + 1, Season.Winter);
                case Season.Winter: return new Quarter(quarterName.Year, Season.Spring);
                case Season.Spring:
                    {
                        //algorithm.takeSummerCourses = true;
                        if (algorithm.takeSummerCourses==true)
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
                case Season.Summer:
                    {
                        return new Quarter(quarterName.Year, Season.Fall);
                    }
                default: return quarterName;
            }
        }

        /// <summary>
        /// method to remove course from list
        /// </summary>
        /// <param name="c"></param>
        /// <returns>the list of courses after removing course</returns>
        public List<Course> RemoveCourse(Course c)
        {
            courses.Remove(c);
            return courses;
        }

        /// <summary>
        /// toString method override to print all the schedules
        /// </summary>
        /// <returns>printed schedule</returns>
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

        /// <summary>
        /// method to check schedule for previous quarter and if previous quarter exists or not
        /// </summary>
        /// <returns>previous quarter schedule if it exists</returns>
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

