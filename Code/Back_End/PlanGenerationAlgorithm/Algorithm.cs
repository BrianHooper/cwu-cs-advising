//import statements
using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Algorithm
    {
        public static void Main(string[] args)
        {
            new Algorithm().Run();
        }
        public void Run()
        {
            //Variables
            double d_minCumulativeGPA = 3.0;
            CatalogCreditRequirements ccr_creditRequirements = new CatalogCreditRequirements();
            uint ui_minQuartersAtCWU = new uint(); //min quarter
            Name n_name = new Name("", "")
            {
                FirstName = "Rico", //student name
                LastName = "Adrian"
            };
            string s_ID = "3829"; //student ID
            uint quarter = 2018; //starting year
            Season s = Season.Fall; //starting quarter
            Quarter q_startingQuarter = new Quarter(quarter, s); //starting quarter
            List<DegreeRequirements> l_degreeRequirements = new List<DegreeRequirements>();
            CatalogRequirements catalog = new CatalogRequirements(s_ID, ui_minQuartersAtCWU,
                d_minCumulativeGPA, ccr_creditRequirements, l_degreeRequirements);
            //quarter(s) when the classes are offered (boolean)
            //{winter,spring,summer,fall}
            bool[] CS111Offered = { true, true, false, false }; //CS111 etc
            bool[] CS110Offered = { true, true, false, true }; //cs110 and math172,CS112 and Math 260
            bool[] CS311Offered = { false, true, false, true }; //CS311, CS301
            bool[] CS312Offered = { true, false, false, true }; //CS312 etc

            //courses and the prerequisites
            ICollection<Course> NoPrereq = new List<Course>();
            NoPrereq.Clear();
            ICollection<Course> Need110 = new List<Course>();
            Course CS110 = new Course("bs", "CS110", 4, true, CS110Offered, NoPrereq);
            Course gened1 = new Course("gened1", "Eng101", 5, true, CS110Offered, NoPrereq);
            Course gened2 = new Course("gened2", "US Cultures", 5, true, CS110Offered, NoPrereq);
            Course gened3 = new Course("gened3", "Philosophies", 5, true, CS110Offered, NoPrereq);
            Course gened4 = new Course("gened4", "Aesthetic", 5, true, CS110Offered, NoPrereq);
            ICollection<Course> NeedEng102 = new List<Course>();
            NeedEng102.Add(gened1);
            Course gened5 = new Course("gened5", "Eng102", 5, true, CS110Offered, NeedEng102);
            Need110.Add(CS110);
            Course CS111 = new Course("bd", "CS111", 4, true, CS111Offered, Need110);
            Course gened6 = new Course("gened6", "Human Behavior", 5, true, CS110Offered, NoPrereq);
            Course gened7 = new Course("gened7", "World Cultures", 5, true, CS110Offered, NoPrereq);
            Course Math172 = new Course("bac", "Math172", 4, true, CS110Offered, NoPrereq);
            Course CS311 = new Course("bq", "CS311", 4, true, CS311Offered, Need110);
            Course CS112 = new Course("bc", "CS112", 4, true, CS110Offered, NoPrereq);
            ICollection<Course> Prereq301 = new List<Course>();
            Prereq301.Add(CS110);
            Prereq301.Add(CS111);
            ICollection<Course> Prereq312 = new List<Course>();
            Prereq312.Add(CS311);
            ICollection<Course> prereqMath260 = new List<Course>();
            prereqMath260.Add(Math172);
            Course CS301 = new Course("ba", "CS301", 4, true, CS311Offered, Prereq301);
            ICollection<Course> prereqCS302 = new List<Course>();
            prereqCS302.Add(Math172);
            prereqCS302.Add(CS301);
            Course CS302 = new Course("bvv", "CS302", 4, true, CS312Offered, prereqCS302);
            Course CS312 = new Course("bb", "CS312", 4, true, CS312Offered, Prereq312);
            Course Math260 = new Course("baa", "Math260", 5, true, CS110Offered, prereqMath260);
            ICollection<Course> prereqMath330 = new List<Course>();
            prereqMath330.Add(Math260);
            Course Math330 = new Course("bcc", "Math 330", 4, true, CS311Offered, prereqMath330);
            List<Course> coursesList = new List<Course>();

            //Add courses to a list of courses
            coursesList.Add(CS110);
            coursesList.Add(gened6);
            coursesList.Add(gened7);
            coursesList.Add(CS312);
            coursesList.Add(CS112);
            coursesList.Add(gened3);
            coursesList.Add(gened4);
            coursesList.Add(CS301);
            coursesList.Add(CS302);
            coursesList.Add(gened5);
            coursesList.Add(Math172);
            coursesList.Add(CS311);
            coursesList.Add(gened1);
            coursesList.Add(gened2);
            coursesList.Add(CS111);
            coursesList.Add(Math260);
            coursesList.Add(Math330);

            //create object schedule and call generate schedule for starting quarter
            Schedule schedule = new Schedule(q_startingQuarter, 0);
            Schedule completed = GenerateSchedule(coursesList, schedule);          
            //while requirements are not completed
            while (completed != null)
            {
                //printing each quarter schedule
                Console.WriteLine(completed.quarterName.QuarterSeason + " " 
                    + completed.quarterName.Year + " schedule :");
                Console.WriteLine("");
                Console.WriteLine(String.Join("\n", completed.courses));
                Console.WriteLine("");

                //skipping summer class
                if (completed.NextQuarter != null)
                {
                    if (completed.quarterName.QuarterSeason == Season.Spring)
                    {
                        completed.quarterName++;
                    }
                    completed.quarterName++;
                    completed.NextQuarter.quarterName = completed.quarterName;
                }
                //generate schedule for next quarter
                completed = completed.NextQuarter;
            }
            Console.ReadKey();
        }

        //method to generate schedule(incomplete)
        public Schedule GenerateSchedule(List<Course> requirements, Schedule currentSchedule)
        {
            //generate possible courses for current quarter by calling ListofCourse method
            List<Course> possibleCourses = ListofCourse(currentSchedule, requirements);

            //if all courses in the requirements are not completed
            if (requirements.Count > 0)
            {
                //if no more possible courses, return all the current schedule
                if (possibleCourses.Count == 0)
                {
                    return currentSchedule;
                }
                else
                {
                    //add class to current schedule based on possible courses
                    //and remove that class added from requirement
                    foreach (Course c in possibleCourses)
                    {
                        if (possibleCourses.Count != 0)
                        {
                            if (currentSchedule.courses.Count < 3 && !currentSchedule.courses.Contains(c)
                                && currentSchedule.quarterName.QuarterSeason != Season.Summer)
                            {
                                currentSchedule.addClass(c);
                                requirements.Remove(c);
                                //call generate schedule recursively until no more possible courses
                                GenerateSchedule(requirements, currentSchedule);
                            }
                        }
                    }
                    //go into next quarter
                    currentSchedule.quarterName++;
                    currentSchedule.nextQuarter().quarterName = currentSchedule.quarterName;
                    //generate schedule for next quarter
                    GenerateSchedule(requirements, currentSchedule.nextQuarter());
                }
            }
            //return all schedule from first to last quarter 
            //if all requirements are completed
            return currentSchedule;
        }

        //method to list possible courses for each quarter
        public List<Course> ListofCourse(Schedule currentQuarter,
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



