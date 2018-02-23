//import statements
using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Algorithm
    {
        //variables
        public static uint minCredits = 10; //set minimum number of credits
        public static uint maxCredits = 18; //set maximum number of credits
        public static Schedule bestSchedule; //variable to save the best possible schedule
        public List<Course> copy= new List<Course>();

        public static Schedule Generate(List<Course> requirements, Schedule currentSchedule, uint minCredits, uint maxCredits)
        {
            minCredits = 10;
            maxCredits = 18;
            Algorithm algorithm = new Algorithm();
            algorithm.GenerateSchedule(requirements, currentSchedule);
            //"\n" + Generated.GetFirstSchedule());
            return bestSchedule;
        }


        //method to generate schedule(incomplete)
        private void GenerateSchedule(List<Course> requirements, Schedule currentSchedule)
        {
            //bestSchedule = currentSchedule;
            copy = new List<Course>(requirements);
            //if there are no requirements left
            if (copy.Count == 0)
            {
                //copy = new List<Course>(requirements);
                if (currentSchedule.NumberOfQuarters <= bestSchedule.NumberOfQuarters)
                {
                    bestSchedule.courses = currentSchedule.courses;
                    bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                    currentSchedule.NumberOfQuarters = 0;
                    //Console.WriteLine("\n" + bestSchedule);
                    //GenerateSchedule(copy, currentSchedule);
                    return;
                }          
                
            }
            //if there are still requirements left
            else if(copy.Count>0)
            {
                bestSchedule = currentSchedule;
                bestSchedule.courses = currentSchedule.courses;
                bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                uint lowerBound = currentSchedule.lowerBound();
                if (lowerBound > bestSchedule.NumberOfQuarters)
                    return;
                //currentSchedule.NumberOfQuarters = 0;
            }
       
         
            // Get a list of each course the student can take right now
            List<Course> possibleCourses = ListofCourse(currentSchedule,requirements);

            //if possible course for this quarter is more than 0
            if (possibleCourses.Count > 0)
            {
                copy = new List<Course>(requirements);
                foreach (Course c in possibleCourses)
                {
                    
                    // Attempt to add the course to this schedule
                    if (maxCredits >= (currentSchedule.ui_numberCredits + c.Credits))
                    {
                        //copy = new List<Course>(requirements);
                        if (currentSchedule.AddCourse(c))
                        {
                            // If it succeeded, adding another course this quarter
                            // requirements.Remove(c);
                            copy.Remove(c);
                            GenerateSchedule(copy, currentSchedule);
                        }
                    }
                    else
                    {
                        currentSchedule.NumberOfQuarters++;
                        // If it failed, try adding this course next quarter
                        GenerateSchedule(copy, currentSchedule.NextSchedule());
                    }
                }
            }
            else if (copy.Count > 0)
            {
                // If there are still requirements left, try again next quarter
                // Danger, could result in infinite recursion until
                // checking for schedule length is implemented
                GenerateSchedule(copy, currentSchedule.NextSchedule());
            }

           // currentSchedule.NumberOfQuarters = 0;
          //return;

        }



        //method to list possible courses for each quarter
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

        //method to check if prerequisites are met or not
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
    }
}



