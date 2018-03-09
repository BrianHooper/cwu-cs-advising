using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        //variables
        public Student student; //initialize student object
        public Quarter quarterName; //initialize quarter object
        private static Random rng = new Random(); //initialize random object
        public bool locked = false; //initialize value of quarter lock
        public uint NumberOfQuarters = 0; //total number of quarters
        public List<Course> courses; //list of all courses taken
        public Schedule NextQuarter, previousQuarter; //initialize next quarter and previous quarter
                                                      //will work like linked list to connect quarters
        public uint ui_numberCredits = 0; //initialize number of credits to 0
        public bool TakeSummerCourses = false; //initialize value of student taking summer courses

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="quarter">current quarter</param>
        public Schedule(Quarter quarter)
        {
            quarterName = quarter; //current quarter name
            courses = new List<Course>(); //initialize course
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
            //meets constraints if course is not on the list of requirements 
            //and number of credits of current schedule is less than 18
            return (ui_numberCredits <= Algorithm.maxCreditss && !courses.Contains(c));
        }

        /// <summary>
        /// method to get the lower bound to check the best possible solution
        /// </summary>
        /// <returns>lower bound</returns>
        public uint lowerBound()
        {
            uint totalRemainingCredits = 0;
            //add course credits to totalRemainingCredits
            foreach (Course c in courses)
            {
                totalRemainingCredits += c.Credits;
            }
            return NumberOfQuarters + (totalRemainingCredits / Algorithm.maxCreditss);
        }

        /// <summary>
        /// method to add course to the list
        /// if the course meets constraints
        /// </summary>
        /// <param name="c">possible course to be added</param>
        /// <returns>true or false depends on whether the course meet all the constraints or not</returns>
        public bool AddCourse(Course c)
        {
            //adding courses can only be possible if constraints are met and quarter is not locked
            //constraints are number of credits is less than maximum credits and if the course is
            //not in the requirements list
            //second argument after || will check specifically for summer quarter since summer quarter
            //default value is false and locked is true
            if ((MeetsConstraints(c) && locked == false) ||
                (quarterName.QuarterSeason == Season.Summer && Algorithm.takeSummerCourses == true))
            {
                courses.Add(c); //add course into the list if it meets all the constraints
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
                //check if next quarter is locked to determine if student take summer courses or not
                if (NextQuarter.quarterName.QuarterSeason.Equals(Season.Summer) && !NextQuarter.locked)
                {
                    Algorithm.takeSummerCourses = true;
                }
                NextQuarter.previousQuarter = this;
            }
            if (NextQuarter.locked)
            {
                //if next quarter is locked, jump into next 2 quarters
                return NextQuarter.NextSchedule();
            }
            return NextQuarter;
        }

        /// <summary>
        /// method to get the schedule for next quarter
        /// </summary>
        /// <returns>go to the next quarter schedule</returns>
        public Schedule NextScheduleSimple()
        {
            Season curSeason = quarterName.QuarterSeason;
            uint curYear = quarterName.Year;
            Quarter next = Quarter.DefaultQuarter;
            //go to next quarter everytime this method is called
            //increment a year if current quarter is fall
            if (curSeason == Season.Fall)
            {
                next = new Quarter(curYear + 1, Season.Winter);
            }
            else if (curSeason == Season.Winter)
            {
                next = new Quarter(curYear, Season.Spring);
            }
            else if (curSeason == Season.Spring)
            {
                next = new Quarter(curYear, Season.Summer);
            }
            else if (curSeason == Season.Summer)
            {
                next = new Quarter(curYear, Season.Fall);
            }

            NextQuarter = new Schedule(next);
            NextQuarter.previousQuarter = this;
            return NextQuarter;
        }

        /// <summary>
        /// method to increment the quarter season and/or year
        /// </summary>
        /// <returns>new quarter with new quarter name and possible new year</returns>
        public Quarter GetNextQuarter()
        {
            Algorithm algorithm = new Algorithm();

            //go to next quarter everytime this method is called
            //increment a year if current quarter is fall
            switch (quarterName.QuarterSeason)
            {
                case Season.Fall: NumberOfQuarters++; return new Quarter(quarterName.Year + 1, Season.Winter);
                case Season.Winter: NumberOfQuarters++; return new Quarter(quarterName.Year, Season.Spring);
                case Season.Spring:
                    {
                        if (Algorithm.takeSummerCourses == true)
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
                case Season.Summer:
                    {
                        NumberOfQuarters++;
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
            //remove course from the list
            courses.Remove(c);
            return courses;
        }

        /// <summary>
        /// shuffle the list of courses order to increase efficiency
        /// </summary>
        /// <typeparam name="Course">data type course</typeparam>
        /// <param name="list">list of courses</param>
        public void Shuffle<Course>(List<Course> list)
        {

            int n = list.Count;
            while (n > 1)
            {
                n--;
                //generate random course
                //from random index
                int k = rng.Next(n + 1);
                Course value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

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

