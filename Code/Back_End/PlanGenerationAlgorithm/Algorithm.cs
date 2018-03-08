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
        int i = 0;
        int j = 0;
        int asd = 0;
        int k = 0;
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
            //foreach (Course req in requirements)
            // {
            //WriteToLog("Algorithm recieved unmet requirement " + req.ID);
            // }
            maxCreditss = maxCredits;
            //set minimum and maximum number of credits
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
            //"\n" + Generated.GetFirstSchedule());
            return bestSchedule;
        }

        /// <summary>
        /// method to generate schedule for each quarter with recursion
        /// </summary>
        /// <param name="requirements">graduation requirements</param>
        /// <param name="currentSchedule">schedule for this quarter</param>
        private void GenerateSchedule(List<Course> requirements, Schedule currentSchedule)
        {
           
            Algorithm algorithm=new Algorithm();
            //Schedule asd = new Schedule(currentSchedule);
            //bestSchedule = currentSchedule;
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
                    copy.Remove(c);
                    //using addCourse and Add are different
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
                    //bestQuarter = currentQuarter;
                    //reset the current schedule
                    // currentSchedule.NumberOfQuarters = 0;
                    return;
                }
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

            //add courses in winter 2019 for testing purposes
            //will be removed when integration is done
            //if (currentSchedule.quarterName.QuarterSeason == Season.Winter && currentSchedule.quarterName.Year == 2019)
            //{
                //ICollection<Course> Need110 = new List<Course>();
               // bool[] CS111Offered = { true, true, false, false }; //CS111 etc
                //Course CS111 = new Course("bd", "CS111", 4, true, CS111Offered, Need110);
                //Course gened1 = new Course("gened1", "Eng101", 5, true, CS111Offered, Need110);
                //Course gened2 = new Course("gened2", "US Cultures", 5, true, CS111Offered, Need110);
                //Course gened3 = new Course("gened2", "asd", 4, true, CS111Offered, Need110);
                //if (j == 0)
                //{
                    // currentSchedule.AddCourse(CS111);
                   // currentSchedule.AddCourse(CS111);
                    //currentSchedule.AddCourse(gened2);
                    //currentSchedule.AddCourse(gened3);
                    //copy.Remove(CS111);
                    //copy.Remove(gened3);
                    //copy.Remove(gened2);
                    //copy.Remove(gened3);

                    //j++;
                //}
            //}
            // Get a list of each course the student can take right now
            List<Course> possibleCourses = ListofCourse(currentSchedule, copy);
            List<Course> prereq = new List<Course>();
            //if possible course for this quarter is more than 0
            foreach (Course c in copy)
            {
                //add courses to a list to prioritize
                //courses that will be required in the future
                foreach (Course hasPrereq in c.PreRequisites)
                {
                    // If it succeeded, adding another course this quarter
                    // requirements.Remove(c);
                    if (!prereq.Contains(hasPrereq))
                    {
                        prereq.Add(hasPrereq);
                    }
                    //Thread.Sleep(1000);
                }
            }
            if (possibleCourses.Count > 0)
            {
               
                //copy = new List<Course>(requirements);
                foreach (Course c in possibleCourses)
                {
                    //if course has prerequisites, prioritize the course first
                    if (prereq.Contains(c) || c.PreRequisites.Count > 0)
                    {
                        // Attempt to add the course to this schedule
                        if (maxCreditss >= (currentSchedule.ui_numberCredits + c.Credits)&&
                            currentSchedule.courses.Count<3)
                        {
                            //copy = new List<Course>(requirements);
                            if (currentSchedule.AddCourse(c))
                            {
                                // If it succeeded, adding another course this quarter
                                // requirements.Remove(c);
                                copy.Remove(c);
                                //prereq.Remove(c);
                                GenerateSchedule(copy, currentSchedule);
                                //Thread.Sleep(1000);
                            }
                        }
                    }
                }
                foreach (Course c in possibleCourses)
                {
                    //if course does not have prerequisites
                    if (c.PreRequisites.Count==0 || c.PreRequisites.Count > 0)
                    {
                        // Attempt to add the course to this schedule
                        if (maxCreditss >= (currentSchedule.ui_numberCredits + c.Credits))
                        {
                            //copy = new List<Course>(requirements);
                            if (currentSchedule.AddCourse(c))
                            {
                                // If it succeeded, adding another course this quarter
                                // requirements.Remove(c);
                                copy.Remove(c);
                                GenerateSchedule(copy, currentSchedule);
                                //Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            else
            {
                //GenerateSchedule(copy, currentSchedule.NextSchedule());
                //copy = new List<Course>(requirements);
                currentQuarter++;
                //bestQuarter = currentQuarter;
                //check if next quarter schedule already have some courses
                //if yes, remove it from requirements and 
                //add the number of credits for next schedule
                foreach (Course c in currentSchedule.NextSchedule().courses)
                {
                    copy.Remove(c);
                    //currentSchedule.NextSchedule().ui_numberCredits += c.Credits;
                }
             
                //if there is no possible course, go to next quarter
                GenerateSchedule(copy, currentSchedule.NextSchedule());
                //return;
                //Thread.Sleep(1000);
            }
            if (copy.Count > 0)
            {
                currentQuarter++;
                //bestQuarter = currentQuarter;
                // If there are still requirements left, try again next quarter
                // Danger, could result in infinite recursion until
                // checking for schedule length is implemented
                foreach (Course c in currentSchedule.NextSchedule().courses)
                {
                    copy.Remove(c);
                    //currentSchedule.NextSchedule().ui_numberCredits += c.Credits;
                }

                //If it failed, try adding this course next quarter
                //it means that course cannot be added this quarter because of 
                //prerequisites problem
                //this will avoid infinite recursion
                GenerateSchedule(copy, currentSchedule.NextSchedule());
                //return;
            }


            //bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
            //bestSchedule.courses = currentSchedule.courses;

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



