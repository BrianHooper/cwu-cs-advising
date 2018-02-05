//import statements
using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Algorithm
    {

        public static void Run()
        {
            //fields
            //current unused variable
            uint ui_numberCredits = new uint();  //number credits
            bool b_requiresMajor = false;
            int minCredits, maxCredits;
            double d_minCumulativeGPA = 3.0;
            CatalogCreditRequirements ccr_creditRequirements = new CatalogCreditRequirements();
            uint ui_minQuartersAtCWU = new uint(); //min quarter
            uint ui_minElectiveCredits = new uint(); //min elective

            //used variable
            Name n_name = new Name("", "")
            {
                FirstName = "Rico", //student name
                LastName = "Adrian"
            };
            string s_ID = "3829"; //student ID
            uint quarter = 2018; //starting year
            Season s = Season.Fall; //starting quarter
            //hashset of course to put courses that are already taken::
            HashSet<Course> coursesTaken = new HashSet<Course>();
            Quarter q_startingQuarter = new Quarter(quarter, s); //starting quarter
            List<DegreeRequirements> l_degreeRequirements = new List<DegreeRequirements>();
            string s_name = "asd";
            Student student1 = new Student(n_name, s_ID, q_startingQuarter);
            Student student2 = new Student(n_name, "3502", q_startingQuarter);
            Student student3 = new Student(n_name, "3253", q_startingQuarter);
            CatalogRequirements catalog = new CatalogRequirements(s_ID, ui_minQuartersAtCWU,
                d_minCumulativeGPA, ccr_creditRequirements, l_degreeRequirements);
            //quarter(s) when the classes are offered (boolean)
            bool[] CS111Offered = { true, true, false, false }; //CS111
            bool[] CS110Offered = { true, true, true, true }; //cs110 and math172,CS112 and Math 260
            bool[] CS311Offered = { false, true, false, true }; //CS311, CS301
            bool[] CS312Offered = { true, false, false, true }; //CS312
            //2 other possible combination(unused)
            bool[] QuartersOffered = { true, true, false, true };
            bool[] QuartersOfferedTwo = { true, true, false, true };

            //courses and the prerequisites
            ICollection<Course> NoPrereq = new List<Course>();
            NoPrereq.Clear();
            ICollection<Course> Need110 = new List<Course>();
            Course CS110 = new Course("bs", "CS110", 4, true, CS110Offered, NoPrereq);
            Course gened1= new Course("gened1", "Eng101", 5, true, CS110Offered, NoPrereq);
            Course gened2= new Course("gened2", "US Cultures", 5, true, CS110Offered, NoPrereq);
            Course gened3 = new Course("gened3", "Philosophies", 5, true, CS110Offered, NoPrereq);
            Course gened4 = new Course("gened4", "Aesthetic", 5, true, CS110Offered, NoPrereq);
            ICollection<Course> NeedEng102 = new List<Course>();
            NeedEng102.Add(gened1);
            Course gened5 = new Course("gened5", "Eng102", 5, true, CS110Offered, NeedEng102);
            Need110.Add(CS110);
            Course gened6 = new Course("gened6", "Human Behavior", 5, true, CS110Offered, NoPrereq);
            Course gened7 = new Course("gened7", "World Cultures", 5, true, CS110Offered, NoPrereq);
            Course Math172 = new Course("bac", "Math172", 4, true, CS110Offered, NoPrereq);
            Course CS111 = new Course("bd", "CS111", 4, true, CS111Offered, Need110);
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

            //temporary unused::
            DegreeRequirements deg1 = new DegreeRequirements(coursesList, coursesList, coursesList, coursesList,
                                 ui_minElectiveCredits, d_minCumulativeGPA, s_name);
            deg1.GeneralRequirements.Append(CS110);
            deg1.GeneralRequirements.Append(CS111);
            deg1.GeneralRequirements.Append(CS311);
            catalog.DegreeRequirements.Append(deg1);

            //Add courses to a list of courses
            coursesList.Add(gened1);
            coursesList.Add(gened2);
            coursesList.Add(gened3);
            coursesList.Add(gened4);
            coursesList.Add(Math172);
            coursesList.Add(CS110);
            coursesList.Add(gened5);
            coursesList.Add(CS111);
            coursesList.Add(gened6);
            coursesList.Add(gened7);
            coursesList.Add(CS311);
            coursesList.Add(CS112);
            coursesList.Add(CS301);
            coursesList.Add(CS302);
            coursesList.Add(CS312);
            coursesList.Add(Math260);
            coursesList.Add(Math330);
            
            Schedule schedule = new Schedule(q_startingQuarter, 0);

            //temporary unused:: until asd.ForEach(Console.WriteLine);
            //schedule.NextQuarter = new Schedule(new Quarter(2018, Season.Winter), 0);
            //schedule.NextQuarter.NextQuarter = new Schedule(new Quarter(2018, Season.Spring), 0);
            //schedule.NextQuarter.NextQuarter.NextQuarter = new Schedule(new Quarter(2019, Season.Fall), 0);
            //schedule.addClass(courses[0]);
            //h.Add(courses[0]);
            //courses.Remove(courses[0]);
            //schedule.NextQuarter.addClass(courses[0]);
            //h.Add(courses[0]);
            //courses.Remove(courses[0]);
            //schedule.NextQuarter.NextQuarter.addClass(courses[0]);
            //h.Add(courses[0]);
            //courses.Remove(courses[0]);
            //List<Course> graduation = new List<Course>();
            //graduation.Add(course);
            //graduation.Add(course5);
            //asd = ListofCourse(schedule, courses, h);
            //asd.ForEach(Console.WriteLine);

            //a for loop to list the schedule in each quarter(12 of them)
            for (int i = 0; i < 12; i++)
            {
                List<Course> possibleCourses = ListofCourse(schedule, coursesList, coursesTaken);
                for (int j = 0; j < possibleCourses.Count; j++)
                {
                    if (schedule.courses1.Count<3)
                    {
                        schedule.addClass(possibleCourses[j]);
                    }
                }
                //print the final schedule of each quarter to the console
                Console.WriteLine(schedule.quarterName.QuarterSeason + " " + schedule.quarterName.Year + " schedule :");
                Console.WriteLine("");
                Console.WriteLine(String.Join("\n", schedule.courses1));
                Console.WriteLine("");
                //add into hashset and remove the current possible Courses
                for (int k = 0; k <schedule.courses1.Count; k++)
                {
                    coursesTaken.Add(schedule.courses1[k]);
                }
                for (int k = 0; k < possibleCourses.Count; k++)
                {
                    schedule.removeClass(possibleCourses[k]);
                }

                schedule.quarterName++; // go to next quarter
            }

            //unused variables for now:: all the way until Console.ReadKey()
            //Console.WriteLine(asd[i]);
            //asd.ForEach(Console.WriteLine);
            // Console.WriteLine(String.Join("\n", h));
            //Console.Write("asd");
            //Print();
            //to do:add courses list with prereq into course list
            //catalog.DegreeRequirement;

            List<Schedule> schedules = new List<Schedule>();
            schedules.Add(schedule);
            List<Student> students = new List<Student>();
            l_degreeRequirements.Add(new DegreeRequirements(coursesList, coursesList, coursesList, coursesList,
                                   ui_minElectiveCredits, d_minCumulativeGPA, s_name));
            l_degreeRequirements.Add(deg1);
            students.Add(student1);
            students.Add(student2);
            students.Add(student3);
            foreach (DegreeRequirements gradReq in catalog.DegreeRequirements)
            {
                foreach (Course course1 in deg1.GeneralRequirements)
                {
                    if (course1.IsOffered(s++))
                    {
                        coursesList.Add(course1);
                    }
                }
            }
            Console.ReadKey();
        }

        //method to generate schedule(incomplete)
        void generateSchedule(Student student, Schedule schedule)
        {
            List<Schedule> schedules = new List<Schedule>();
            if (student.CreditsCompleted.Equals(true))
            {
                schedules.Add(schedule);
                return;
            }
            else
            {

            }
            if (!schedules.Any())
            {
                generateSchedule(student, schedule);
            }
            else
            {
                //foreach(Course c)
            }
        }

        //method to list possible courses for each quarter
        public static List<Course> ListofCourse(Schedule currentQuarter,
            List<Course> graduation, HashSet<Course> courseList)
        {
            List<Course> possibleCourses = new List<Course>();
            foreach (Course c in graduation)
            {
                if (!courseList.Contains(c) && c.IsOffered(currentQuarter.quarterName.QuarterSeason)
                    && prereqsMet(c, courseList))
                {

                    possibleCourses.Add(c);
                }
            }
            return possibleCourses;
        }

        //method to check if prerequisites are met or not
        public static Boolean prereqsMet(Course c, HashSet<Course> courseList)
        {
            foreach (Course course in c.PreRequisites)
            {
                if (!courseList.Contains(course))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
