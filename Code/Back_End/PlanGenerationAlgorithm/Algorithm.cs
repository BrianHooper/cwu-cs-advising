//import statements
using System;
using System.Threading;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Algorithm
    {
        //variables
        public static uint minCredits = 10; //set minimum number of credits
        public static uint maxCreditss; //set maximum number of credits
        public static Schedule bestSchedule; //variable to save the best possible schedule
        public List<Course> copy;  //a list to copy the requirements in generateSchedule method
        public static bool takeSummerCourses; //variable to check if student take summer courses
        Schedule schedule = new Schedule(new Quarter(2018, Season.Fall));
        public static uint bestQuarter = 50;
        public uint currentQuarter = 0;
        //variable for increment
        int i = 0; //variable to initialize the best schedule in the GenerateSchedule function

        /// <summary>
        /// call the generate schedule method
        /// and generate the schedules for all graduation requirements
        /// </summary>
        /// <param name="requirements">graduation requirements</param>
        /// <param name="currentSchedule">schedule for this quarter</param>
        /// <param name="minCredits">minimum possible number of credits</param>
        /// <param name="maxCredits">maximum possible number of credits</param>
        /// <param name="takeSummerCourse">check if student take summer courses</param>
        /// <returns>best possible schedule</returns>
        public static Schedule Generate(List<Course> requirements, Schedule currentSchedule, uint minCredits, uint maxCredits, bool takeSummerCourse)
        {
            //set maximum number of credits to the input number on the front end
            maxCreditss = maxCredits;
            //set the value of take summer course to determine 
            //if student takes summer courses or not
            Algorithm algorithm = new Algorithm();
            takeSummerCourses = takeSummerCourse;

            //initialize the values of best schedule
            bestSchedule = currentSchedule;
            bestSchedule.NextQuarter = currentSchedule.NextQuarter;
            bestSchedule.previousQuarter = currentSchedule.previousQuarter;
            bestSchedule.ui_numberCredits = currentSchedule.ui_numberCredits;
            bestSchedule.quarterName = currentSchedule.quarterName;
            bestSchedule.courses = currentSchedule.courses;

            //set best schedule number of quarters to 50 for first iteration
            bestSchedule.NumberOfQuarters = 50;

            for (int i = 0; i < 15; i++)
            {
                //shuffle the order of courses
                currentSchedule.Shuffle(requirements);
                //generate the schedule
                algorithm.GenerateSchedule(requirements, currentSchedule);
                if (algorithm.currentQuarter <= bestQuarter)
                {
                    //set best schedule to current schedule if 
                    //current number of quarter is less than current best quarter
                    bestSchedule = currentSchedule;
                    bestSchedule.NextQuarter = currentSchedule.NextQuarter;
                    bestSchedule.previousQuarter = currentSchedule.previousQuarter;
                    bestSchedule.ui_numberCredits = currentSchedule.ui_numberCredits;
                    bestSchedule.quarterName = currentSchedule.quarterName;
                    bestSchedule.courses = currentSchedule.courses;
                    bestQuarter = algorithm.currentQuarter;
                    //reset the current schedule

                }

            }
            //return best schedule generated from algorithm
            return bestSchedule;
        }

        /// <summary>
        /// method to generate schedule for each quarter with recursion
        /// </summary>
        /// <param name="requirements">graduation requirements</param>
        /// <param name="currentSchedule">schedule for this quarter</param>
        private void GenerateSchedule(List<Course> requirements, Schedule currentSchedule)
        {

            //instantiate algorithm object
            Algorithm algorithm = new Algorithm();
            //copy all the courses in the requirements using copy constructor so 
            //that the algorithm can access the requirements
            //without changing them everytime
            copy = new List<Course>(requirements);
            // currentSchedule = new Schedule(new Quarter(2018, Season.Fall));
            if (i == 0)
            {
                //initialize variable for best possible schedule
                bestSchedule = currentSchedule;
                bestSchedule.NextQuarter = currentSchedule.NextQuarter;
                bestSchedule.previousQuarter = currentSchedule.previousQuarter;
                bestSchedule.ui_numberCredits = currentSchedule.ui_numberCredits;
                bestSchedule.quarterName = currentSchedule.quarterName;
                bestSchedule.courses = currentSchedule.courses;
                bestQuarter = 50;
                foreach (Course c in currentSchedule.courses)
                {
                    //if there are courses in the current schedule,
                    //remove them from the requirements
                    //number of credits are already added in the front end
                    //no need to increment number of credits

                    copy.Remove(c);
                    //Note: using addCourse and Add are different
                    //addCourse add current schedule number of credits, Add does not
                    //adding number of credits if Add is used and 
                    //do not add number of credits if AddCourse is used
                    //currentSchedule.ui_numberCredits += c.Credits;
                }
                i++;
            }

            //if there are no requirements left
            if (copy.Count == 0)
            {
                //check if current schedule is better than current best schedule
                if (algorithm.currentQuarter <= bestQuarter)
                {
                    bestSchedule = currentSchedule;
                    bestSchedule.NextQuarter = currentSchedule.NextQuarter;
                    bestSchedule.previousQuarter = currentSchedule.previousQuarter;
                    bestSchedule.ui_numberCredits = currentSchedule.ui_numberCredits;
                    bestSchedule.quarterName = currentSchedule.quarterName;
                    bestSchedule.courses = currentSchedule.courses;
                    return;
                }
                //return if there are no requirements left
                return;
            }
            else
            {
                //if algorithm is still running, check if lower bound for current schedule is
                //worst than best schedule so it does not check the whole tree
                if (bestQuarter != 50)
                {
                    uint lowerBound = currentSchedule.lowerBound();
                    if (lowerBound > bestQuarter)
                        return;
                }
            }

            // Get a list of each course the student can take right now
            List<Course> possibleCourses = ListofCourse(currentSchedule, copy);
            //create a list of prerequisites
            List<Course> prereq = new List<Course>();

            //if possible course for this quarter is more than 0
            foreach (Course c in copy)
            {

                //add courses to a list to prioritize
                //courses that will be required in the future
                //or that are prerequisites of other courses
                foreach (Course hasPrereq in c.PreRequisites)
                {

                    // If it succeeded, adding another course this quarter
                    // requirements.Remove(c);
                    if (!prereq.Contains(hasPrereq))
                    {
                        prereq.Add(hasPrereq);
                    }
                }
            }
            if (possibleCourses.Count > 0)
            {

                foreach (Course c in possibleCourses)
                {

                    //if course has prerequisites, prioritize the course first
                    //or courses are prerequisites of future courses
                    if (prereq.Contains(c) || c.PreRequisites.Count > 0)
                    {
                        // Attempt to add the course to this schedule
                        if (maxCreditss >= (currentSchedule.ui_numberCredits + c.Credits) &&
                            currentSchedule.courses.Count < 4)
                        {
                            if (currentSchedule.AddCourse(c))
                            {
                                // If it succeeded, adding another course this quarter
                                //and remove the course from the requirements
                                copy.Remove(c);

                                //generate another courses and add them into schedule
                                GenerateSchedule(copy, currentSchedule);
                            }
                        }
                    }
                }
                foreach (Course c in possibleCourses)
                {
                    //if course does not have prerequisites
                    //and have prerequisites(basically all possible courses for current quarter)
                    //to avoid infinite recursion
                    if (c.PreRequisites.Count == 0 || c.PreRequisites.Count > 0)
                    {
                        // Attempt to add the course to this schedule
                        if (maxCreditss >= (currentSchedule.ui_numberCredits + c.Credits))
                        {
                            if (currentSchedule.AddCourse(c))
                            {
                                // If it succeeded, adding another course this quarter
                                copy.Remove(c);

                                //generate another courses and add them into schedule
                                GenerateSchedule(copy, currentSchedule);
                            }
                        }
                    }
                }
            }
            else
            {
                //increment current number of quarters
                currentQuarter++;

                //check if next quarter schedule already have some courses
                //if yes, remove it from requirements
                foreach (Course c in currentSchedule.NextSchedule().courses)
                {
                    copy.Remove(c);
                }

                //if there is no possible course for current quarter, go to next quarter
                GenerateSchedule(copy, currentSchedule.NextSchedule());
            }

            //check if there are still requirements left
            //this will happen sometimes because there is a possibility
            //that there are still requirements but there is no more possible courses
            //this will avoid infinite recursion
            if (copy.Count > 0)
            {
                //increment total number of quarters to graduate
                currentQuarter++;

                // If there are still requirements left, try again next quarter
                // Danger, could result in infinite recursion until
                // checking for schedule length is implemented
                foreach (Course c in currentSchedule.NextSchedule().courses)
                {
                    copy.Remove(c);
                }

                //If it failed, try adding this course next quarter
                //it means that course cannot be added this quarter because of 
                //prerequisites problem
                //this will avoid infinite recursion
                GenerateSchedule(copy, currentSchedule.NextSchedule());
            }

            //return if generating schedule is finished and there are no more requirements
            return;

        }


        /// <summary>
        /// method to list possible courses for current quarter
        /// </summary>
        /// <param name="currentQuarter">current quarter to check course offered and prereqs met</param>
        /// <param name="graduation">graduation requirement courses</param>
        /// <returns>all lists of courses that the student can take this quarter</returns>
        private List<Course> ListofCourse(Schedule currentQuarter,
            List<Course> graduation)
        {
            List<Course> possibleCourses = new List<Course>();
            foreach (Course c in graduation)
            {
                //if course is offered and prereqs is met,
                //add course to possible courses
                if (c.IsOffered(currentQuarter.quarterName.QuarterSeason)
                    && prereqsMet(c, currentQuarter))
                {
                    possibleCourses.Add(c);
                }
            }

            //return all possible courses
            return possibleCourses;
        }

        /// <summary>
        /// method to check if prerequisites are met or not
        /// </summary>
        /// <param name="c">all the courses needed for graduation</param>
        /// <param name="currentQuarter">current quarter to check course offered and prereqs met</param>
        /// <returns></returns>
        public Boolean prereqsMet(Course c, Schedule currentQuarter)
        {
            List<Course> coursesTaken = new List<Course>();
            Schedule iterator = currentQuarter;

            while (iterator != null)
            {
                //iterate through courses in the current quarter
                //and add the courses to coursesTaken
                foreach (Course course in iterator.courses)
                {
                    coursesTaken.Add(course);
                }

                //check from all previous quarter, 
                //which courses are already taken
                iterator = iterator.previousQuarter;
            }

            foreach (Course prereq in c.PreRequisites)
            {
                //if coursestaken does not contain prereq
                //or prereq is in the current quarter,
                //return false
                if (!coursesTaken.Contains(prereq) || currentQuarter.courses.Contains(prereq))
                {
                    return false;
                }
            }
            //return true if prereq is met
            return true;
        }

        /// <summary>Writes to log file</summary>
        /// <param name="message">The error message to write</param>
        public static void WriteToLog(string message)
        {
            System.IO.File.AppendAllText("/var/aspnetcore/wwwroot/log.txt", DateTime.Now.ToLongTimeString() + "   --   " + message + "\n");
        }
    }
}



