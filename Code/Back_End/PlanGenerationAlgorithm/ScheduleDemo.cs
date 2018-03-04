using System;
using System.Collections.Generic;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    //Class for testing purposes
    class ScheduleDemo
    {
        public static void Main(string[] args)
        {
            new ScheduleDemo().Run();
        }

        public void Run()
        {
            Console.WriteLine("6 Courses, Max 3 per quarter, no prereqs, each course offered every quarter:");
            DemoWithNoConstraints();

            Console.WriteLine("\n------------------\n");

            Console.WriteLine("3 Courses, Max 3 per quarter, all prereqs, first course not offered in fall:");
            DemoWithAllPrereqs();
            DemoFullSchedule();
            Console.ReadKey();
            Console.WriteLine("End of demo.");
        }

        public void DemoWithAllPrereqs()
        {
            // List of course requirements
            List<Course> Requirements = new List<Course>();
            Student student;
            // Create 3 courses
            List<Course> CS311Prereqs = new List<Course>();
            Requirements.Add(new Course("Computer Architecture", "CS311", 4, true, new bool[] { true, true, false, true }, CS311Prereqs));

            List<Course> CS427Prereqs = new List<Course>();
            CS427Prereqs.Add(Requirements[0]);
            Requirements.Add(new Course("Algorithm Analysis", "CS427", 4, true, new bool[] { true, true, false, true }, CS427Prereqs));

            List<Course> CS470Prereqs = new List<Course>();
            CS470Prereqs.Add(Requirements[0]);
            CS470Prereqs.Add(Requirements[1]);
            Requirements.Add(new Course("Operationg Systems", "CS470", 4, true, new bool[] { true, true, false, true }, CS470Prereqs));
            Course gened5 = new Course("gened1", "111", 5, true, new bool[] { true, true, false, true }, CS311Prereqs);
            Course gened2 = new Course("gened2", "US Cultures", 5, true, new bool[] { true, true, false, true }, CS311Prereqs);
            Course gened3 = new Course("gened3", "Philosophies", 4, true, new bool[] { true, true, false, true }, CS311Prereqs);
            Course gened4 = new Course("gened4", "Aesthetic", 5, true, new bool[] { true, true, false, true }, CS311Prereqs);
            //Requirements.Add(gened5);
            //Requirements.Add(gened2);
            //Requirements.Add(gened3);
            //Requirements.Add(gened4);
            // Write the requirements list to the console
            Console.WriteLine("Course Requirements:");
            foreach (Course c in Requirements)
            {
                Console.Write(c.ID + "  --  Prereq:");
                foreach (Course prereq in c.PreRequisites)
                {
                    Console.Write(" " + prereq.ID);
                }
                Console.Write("\n");
            }

            // Create an empty Schedule starting Fall 2018
            Schedule StudentSchedule = new Schedule(new Quarter(2018, Season.Fall));
            //StudentSchedule.AddCourse(gened5);
            //StudentSchedule.AddCourse(gened2);
            //StudentSchedule.AddCourse(gened4);
            //StudentSchedule.AddCourse(gened3);
            // Run the algorithm
            Schedule GeneratedSchedule = Algorithm.Generate(Requirements, StudentSchedule, 10, 18, true);

            // Output the results to the console
            Console.WriteLine("\n" + GeneratedSchedule.GetFirstSchedule());
        }

        /*
         * Demos populating an empty Schedule object with 6 courses,
         * each of which is offered Fall, Winter, and Spring,
         * and has no prerequisites.
         */
        public void DemoWithNoConstraints()
        {
            // Create a list of 6 courses, each of which is offered fall, winter, and spring, and have no prerequisites
            List<Course> Requirements = new List<Course>();
            List<Course> PreReqs = new List<Course>();
            PreReqs.Clear();
            List<Course> PreReqs1 = new List<Course>();
            Course gened1 = new Course("gened1", "Eng101", 5, true, new bool[] { true, true, false, true }, PreReqs);
            PreReqs1.Add(gened1);
            Requirements.Add(gened1);
            Requirements.Add(new Course("Computer Architecture", "CS311", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Requirements.Add(new Course("Algorithm Analysis", "CS427", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Requirements.Add(new Course("Operationg Systems", "CS470", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Requirements.Add(new Course("Software Engineering", "CS480", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Requirements.Add(new Course("Introduction to UNIX", "CS370", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Requirements.Add(new Course("Programming Languages", "CS361", 4, true, new bool[] { true, true, false, true }, PreReqs));
            Course gened5 = new Course("gened1", "111", 5, true, new bool[] { true, true, false, true }, PreReqs);
            Course gened2 = new Course("gened2", "US Cultures", 5, true, new bool[] { true, true, false, true }, PreReqs);
            Course gened3 = new Course("gened3", "Philosophies", 4, true, new bool[] { true, true, false, true }, PreReqs);
            Course gened4 = new Course("gened4", "Aesthetic", 5, true, new bool[] { true, true, false, true }, PreReqs);
            Requirements.Add(gened5);
            Requirements.Add(gened2);
            Requirements.Add(gened3);
            Requirements.Add(gened4);
            // Write the requirements list to the console
            Console.WriteLine("Course Requirements:");
            foreach (Course c in Requirements)
            {
                Console.WriteLine(c.ID);
            }

            // Create an empty Schedule starting Fall 2018
            Schedule StudentSchedule = new Schedule(new Quarter(2018, Season.Fall));
            StudentSchedule.AddCourse(gened5);
            StudentSchedule.AddCourse(gened2);
            StudentSchedule.AddCourse(gened4);
            StudentSchedule.AddCourse(gened3);
            // Run the algorithm
            Schedule GeneratedSchedule = Algorithm.Generate(Requirements, StudentSchedule, 10, 18, true);

            // Output the results to the console
            Console.WriteLine("\n" + GeneratedSchedule.GetFirstSchedule());
        }

        public void DemoFullSchedule()
        {
            //Variables
            double d_minCumulativeGPA = 3.0;
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
            Course gened3 = new Course("gened3", "Philosophies", 4, true, CS110Offered, NoPrereq);
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
            //Schedule schedule = new Schedule(q_startingQuarter);
            // Schedule completed = Algorithm.Generate(coursesList, schedule);
            //while requirements are not completed

            Console.WriteLine("Course Requirements:");
            foreach (Course c in coursesList)
            {
                Console.Write(c.ID + "  --  Prereq:");
                foreach (Course prereq in c.PreRequisites)
                {
                    Console.Write(" " + prereq.ID);
                }
                Console.Write("\n");
            }

            // Create an empty Schedule starting Fall 2018
            Schedule StudentSchedule = new Schedule(new Quarter(2018, Season.Fall));
            //using addCourse and Add are different
            //addCourse add current schedule number of credits, Add does not

            StudentSchedule.courses.Add(CS110);
            StudentSchedule.courses.Add(Math172);
            // Run the algorithm
            Schedule GeneratedSchedule = Algorithm.Generate(coursesList, StudentSchedule, 10, 18, true);

            // Output the results to the console
            Console.WriteLine("\n" + GeneratedSchedule.GetFirstSchedule());
            Console.WriteLine("\n" + "asdsadsad");

        }
    }
}
