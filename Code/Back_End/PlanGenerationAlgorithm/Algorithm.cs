//import statements
using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Algorithm
    {
        public static Schedule Generate(List<Course> requirements, Schedule currentSchedule)
        {
            Algorithm algorithm = new Algorithm();
            return algorithm.GenerateSchedule(requirements, currentSchedule);
        }

        //method to generate schedule(incomplete)
        private Schedule GenerateSchedule(List<Course> requirements, Schedule currentSchedule)
        {
            // Get a list of each course the student can take right now
            List<Course> possibleCourses = ListofCourse(currentSchedule, requirements);

            if (possibleCourses.Count > 0)
            {
                foreach (Course c in possibleCourses)
                {
                    // Attempt to add the course to this schedule
                    if (currentSchedule.AddCourse(c))
                    {
                        // If it succeeded, adding another course this quarter
                        requirements.Remove(c);
                        return GenerateSchedule(requirements, currentSchedule);
                    }
                    else
                    {
                        // If it failed, try adding this course next quarter
                        return GenerateSchedule(requirements, currentSchedule.NextSchedule());
                    }
                }
            }
            else if (requirements.Count > 0)
            {
                // If there are still requirements left, try again next quarter
                // Danger, could result in infinite recursion until
                // checking for schedule length is implemented
                return GenerateSchedule(requirements, currentSchedule.NextSchedule());
            }
            return currentSchedule;

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



