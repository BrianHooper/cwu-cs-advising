﻿//import statements
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
        public List<Course> copy;
        Schedule schedule = new Schedule(new Quarter(2018, Season.Fall));
        int i = 0;
        /// <summary>
        /// call the generate schedule method
        /// and generate the schedules for all graduation requirements
        /// </summary>
        /// <param name="requirements">graduation requirements</param>
        /// <param name="currentSchedule">schedule for this quarter</param>
        /// <param name="minCredits">minimum possible number of credits</param>
        /// <param name="maxCredits">maximum possible number of credits</param>
        /// <returns>best possible schedule</returns>
        public static Schedule Generate(List<Course> requirements, Schedule currentSchedule, uint minCredits, uint maxCredits)
        {
            minCredits = 10;
            maxCredits = 18;
            Algorithm algorithm = new Algorithm();
            algorithm.GenerateSchedule(requirements, currentSchedule);
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
                bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                i++;
            }
            //if there are no requirements left
            if (copy.Count == 0)
            {
                //check if current schedule is better than current best schedule
                if (currentSchedule.NumberOfQuarters < bestSchedule.NumberOfQuarters)
                {
                    bestSchedule = currentSchedule;
                    bestSchedule.NextQuarter = currentSchedule.NextQuarter;
                    bestSchedule.previousQuarter = currentSchedule.previousQuarter;
                    bestSchedule.ui_numberCredits = currentSchedule.ui_numberCredits;
                    bestSchedule.quarterName = currentSchedule.quarterName;
                    bestSchedule.courses = currentSchedule.courses;
                    bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                    //reset the current schedule
                    currentSchedule.NumberOfQuarters = 0;
                    return;
                }

            }
            else
            {
                //if algorithm is still running, check if lower bound for current schedule is
                //worst than best schedule so it does not check the whole tree
                uint lowerBound = currentSchedule.lowerBound();
                if (lowerBound > bestSchedule.NumberOfQuarters)
                    return;
            }

            // Get a list of each course the student can take right now
            List<Course> possibleCourses = ListofCourse(currentSchedule, copy);

            //if possible course for this quarter is more than 0
            if (possibleCourses.Count > 0)
            {
                //copy = new List<Course>(requirements);
                foreach (Course c in possibleCourses)
                {
                    // Attempt to add the course to this schedule
                    if (maxCredits >= (currentSchedule.ui_numberCredits + c.Credits))
                    {
                        copy = new List<Course>(requirements);
                        if (currentSchedule.AddCourse(c))
                        {
                            // If it succeeded, adding another course this quarter
                            // requirements.Remove(c);
                            copy.Remove(c);
                            GenerateSchedule(copy, currentSchedule);
                        }

                    }

                }
            }
            else
            {
                copy = new List<Course>(requirements);
                currentSchedule.NumberOfQuarters++;
                bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                //If it failed, try adding this course next quarter
                GenerateSchedule(copy, currentSchedule.NextSchedule());
            }
            if (copy.Count > 0)
            {
                currentSchedule.NumberOfQuarters++;
                bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
                // If there are still requirements left, try again next quarter
                // Danger, could result in infinite recursion until
                // checking for schedule length is implemented
                GenerateSchedule(copy, currentSchedule.NextSchedule());
            }

            bestSchedule.NumberOfQuarters = currentSchedule.NumberOfQuarters;
            //bestSchedule.courses = currentSchedule.courses;
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
    }
}



