﻿using System;
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
            DemoFinishedSchedule();
            /*
            Console.WriteLine("6 Courses, Max 3 per quarter, no prereqs, each course offered every quarter:");
            DemoWithNoConstraints();
            Console.WriteLine("\n------------------\n");
            Console.WriteLine("3 Courses, Max 3 per quarter, all prereqs, first course not offered in fall:");
            DemoWithAllPrereqs();
            DemoFullSchedule();
            Console.ReadKey();
            Console.WriteLine("End of demo.");
            */
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
            bool[] CS361Offered = { false, false, false, true };
            bool[] CS470Offered = { true, false, false, false };
            bool[] CS380Offered = { false, true, false, false };
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
            Course gened8 = new Course("vxzbx", "Natural Sciences", 5, true, CS110Offered, NoPrereq);
            Course gened9 = new Course("vsb", "Reasoning", 5, true, CS110Offered, NoPrereq);
            Course gened10 = new Course("qwef", "Literature", 5, true, CS110Offered, NoPrereq);
            Course gened11 = new Course("bac", "UNIV 101", 5, true, CS110Offered, NoPrereq);
            ICollection<Course> Prereq325 = new List<Course>();
            Prereq325.Add(CS301);
            Prereq325.Add(gened5);
            ICollection<Course> Prereq361 = new List<Course>();
            Prereq361.Add(CS302);
            Course CS325 = new Course("bq", "CS325", 4, true, CS110Offered, Prereq325);
            ICollection<Course> Prereq470 = new List<Course>();
            Prereq470.Add(CS302);
            Prereq470.Add(CS312);
            Prereq470.Add(CS325);
            ICollection<Course> Prereq427 = new List<Course>();
            Prereq427.Add(CS302);
            Prereq427.Add(Math330);
            Prereq427.Add(CS325);
            Course CS361 = new Course("bc", "CS361", 4, true, CS361Offered, Prereq361);
            Course CS380 = new Course("qwrwhrtr", "CS380", 4, true, CS380Offered, Prereq361);
            Course CS470 = new Course("dsjtju", "CS470", 4, true, CS470Offered, Prereq470);
            Course CS427 = new Course("byweewh", "CS427", 4, true, CS312Offered, Prereq427);
            ICollection<Course> Prereq480 = new List<Course>();
            Prereq480.Add(CS380);
            Prereq480.Add(CS325);
            Course CS480 = new Course("bjtyre", "CS480", 4, true, CS361Offered, Prereq480);

            ICollection<Course> Prereq481 = new List<Course>();
            Prereq481.Add(CS325);
            Prereq481.Add(CS480);
            ICollection<Course> Prereq362 = new List<Course>();
            Prereq362.Add(CS361);
            Prereq362.Add(Math260);
            Prereq362.Add(CS325);
            Course CS362 = new Course("zxvb", "CS362", 4, true, CS470Offered, Prereq362);
            Course CS481 = new Course("asdg", "CS481", 4, true, CS470Offered, Prereq481);

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
            coursesList.Add(CS481);
            coursesList.Add(CS362);
            coursesList.Add(gened8);
            coursesList.Add(gened9);
            coursesList.Add(CS361);
            coursesList.Add(CS480);
            coursesList.Add(gened10);
            coursesList.Add(gened11);
            coursesList.Add(CS427);
            coursesList.Add(CS470);
            coursesList.Add(CS325);
            coursesList.Add(CS380);
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

            //StudentSchedule.courses.Add(CS110);
            // Run the algorithm
            Schedule GeneratedSchedule = Algorithm.Generate(coursesList, StudentSchedule, 0, 18, true);

            // Output the results to the console
            Console.WriteLine("\n" + GeneratedSchedule.GetFirstSchedule());
            Console.WriteLine("\n" + "asdsadsad");

        }

        public void DemoFinishedSchedule()
        {
            List<Course> NoPrereqs = new List<Course>();

            // Summer 2018
            Schedule schedule = new Schedule(new Quarter(2018, Season.Summer));
            schedule.locked = true;

            // Fall 2018
            schedule = schedule.NextScheduleSimple();
            Course UNIV101 = new Course("UNIV101", "UNIV101", 1, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(UNIV101); schedule.ui_numberCredits += UNIV101.Credits;
            Course MATH153 = new Course("MATH153", "MATH153", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(MATH153); schedule.ui_numberCredits += MATH153.Credits;
            Course ENG101 = new Course("ENG101", "ENG101", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(ENG101); schedule.ui_numberCredits += ENG101.Credits;
            Course CS110 = new Course("CS110", "CS110", 4, true, new bool[] { true, true, false, true }, NoPrereqs); schedule.courses.Add(CS110); schedule.ui_numberCredits += CS110.Credits;

            // Winter 2019
            schedule = schedule.NextScheduleSimple();
            Course MATH154 = new Course("MATH154", "MATH154", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(MATH154); schedule.ui_numberCredits += MATH154.Credits;
            List<Course> CS111Prereqs = new List<Course>(); CS111Prereqs.Add(CS110);
            Course CS111 = new Course("CS111", "CS111", 4, true, new bool[] { true, true, false, false }, CS111Prereqs); schedule.courses.Add(CS111); schedule.ui_numberCredits += CS111.Credits;
            List<Course> ENG102Prereqs = new List<Course>(); ENG102Prereqs.Add(ENG101);
            Course ENG102 = new Course("ENG102", "ENG102", 5, true, new bool[] { true, true, true, true }, ENG102Prereqs); schedule.courses.Add(ENG102); schedule.ui_numberCredits += ENG102.Credits;

            // Spring 2019
            schedule = schedule.NextScheduleSimple();
            List<Course> MATH172Prereqs = new List<Course>(); MATH172Prereqs.Add(MATH154);
            Course MATH172 = new Course("MATH172", "MATH172", 5, true, new bool[] { true, true, true, true }, MATH172Prereqs); schedule.courses.Add(MATH172); schedule.ui_numberCredits += MATH172.Credits;
            List<Course> CS112Prereqs = new List<Course>(); CS112Prereqs.Add(CS111);
            Course CS112 = new Course("CS112", "CS112", 4, true, new bool[] { false, false, true, false }, CS112Prereqs); schedule.courses.Add(CS112); schedule.ui_numberCredits += CS112.Credits;
            Course BREADTH1 = new Course("BREADTH1", "BREADTH1", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH1); schedule.ui_numberCredits += BREADTH1.Credits;

            // Summer 2019
            schedule = schedule.NextScheduleSimple();
            schedule.locked = true;

            // Fall 2019
            schedule = schedule.NextScheduleSimple();
            List<Course> CS301Prereqs = new List<Course>(); CS301Prereqs.Add(CS111); CS301Prereqs.Add(MATH154);
            Course CS301 = new Course("CS301", "CS301", 4, true, new bool[] { false, true, false, true }, CS301Prereqs); schedule.courses.Add(CS301); schedule.ui_numberCredits += CS301.Credits;
            List<Course> CS311Prereqs = new List<Course>(); CS311Prereqs.Add(CS110);
            Course CS311 = new Course("CS311", "CS311", 4, true, new bool[] { false, true, false, true }, NoPrereqs); schedule.courses.Add(CS311); schedule.ui_numberCredits += CS311.Credits;
            Course BREADTH2 = new Course("BREADTH2", "BREADTH2", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH2); schedule.ui_numberCredits += BREADTH2.Credits;
            Course BREADTH3 = new Course("BREADTH3", "BREADTH3", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH3); schedule.ui_numberCredits += BREADTH3.Credits;

            // Winter 2020
            schedule = schedule.NextScheduleSimple();
            List<Course> CS302Prereqs = new List<Course>(); CS302Prereqs.Add(CS301); CS302Prereqs.Add(MATH172);
            Course CS302 = new Course("CS302", "CS302", 4, true, new bool[] { true, false, false, true }, NoPrereqs); schedule.courses.Add(CS302); schedule.ui_numberCredits += CS302.Credits;
            List<Course> CS312Prereqs = new List<Course>(); CS312Prereqs.Add(CS301); CS312Prereqs.Add(CS311);
            Course CS312 = new Course("CS312", "CS312", 4, true, new bool[] { true, false, false, true }, CS312Prereqs); schedule.courses.Add(CS312); schedule.ui_numberCredits += CS312.Credits;
            List<Course> CS325Prereqs = new List<Course>(); CS325Prereqs.Add(CS301); CS325Prereqs.Add(ENG102);
            Course CS325 = new Course("CS325", "CS325", 3, true, new bool[] { false, false, true, false }, CS325Prereqs); schedule.courses.Add(CS325); schedule.ui_numberCredits += CS325.Credits;
            Course BREADTH4 = new Course("BREADTH4", "BREADTH4", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH4); schedule.ui_numberCredits += BREADTH4.Credits;

            // Spring 2020
            schedule = schedule.NextScheduleSimple();
            List<Course> MATH260Prereqs = new List<Course>(); MATH260Prereqs.Add(MATH172); MATH260Prereqs.Add(CS301);
            Course MATH260 = new Course("MATH260", "MATH260", 5, true, new bool[] { false, false, true, false }, MATH260Prereqs); schedule.courses.Add(MATH260); schedule.ui_numberCredits += MATH260.Credits;
            List<Course> CS446Prereqs = new List<Course>(); CS446Prereqs.Add(CS302);
            Course CS446 = new Course("CS446", "CS446", 4, true, new bool[] { false, true, false, false }, CS446Prereqs); schedule.courses.Add(CS446); schedule.ui_numberCredits += CS446.Credits;
            Course BREADTH5 = new Course("BREADTH5", "BREADTH5", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH5); schedule.ui_numberCredits += BREADTH5.Credits;

            // Summer 2020
            schedule = schedule.NextScheduleSimple();
            schedule.locked = true;

            // Fall 2020
            schedule = schedule.NextScheduleSimple();
            List<Course> CS361Prereqs = new List<Course>(); CS361Prereqs.Add(CS302);
            Course CS361 = new Course("CS361", "CS361", 4, true, new bool[] { false, false, false, true }, CS361Prereqs); schedule.courses.Add(CS361); schedule.ui_numberCredits += CS361.Credits;
            List<Course> MATH330Prereqs = new List<Course>(); MATH330Prereqs.Add(MATH260);
            Course MATH330 = new Course("MATH330", "MATH330", 5, true, new bool[] { false, true, false, true }, MATH330Prereqs); schedule.courses.Add(MATH330); schedule.ui_numberCredits += MATH330.Credits;
            Course BREADTH6 = new Course("BREADTH6", "BREADTH6", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH6); schedule.ui_numberCredits += BREADTH6.Credits;

            // Winter 2021
            schedule = schedule.NextScheduleSimple();
            List<Course> CS362Prereqs = new List<Course>(); CS362Prereqs.Add(CS361); CS362Prereqs.Add(MATH260);
            Course CS362 = new Course("CS362", "CS362", 4, true, new bool[] { true, false, false, false }, CS362Prereqs); schedule.courses.Add(CS362); schedule.ui_numberCredits += CS362.Credits;
            List<Course> CS470Prereqs = new List<Course>(); CS470Prereqs.Add(CS302); CS470Prereqs.Add(CS312); CS470Prereqs.Add(CS325);
            Course CS470 = new Course("CS470", "CS470", 4, true, new bool[] { true, false, false, false }, CS470Prereqs); schedule.courses.Add(CS470); schedule.ui_numberCredits += CS470.Credits;
            List<Course> CSElectivePrereqs = new List<Course>(); CSElectivePrereqs.Add(CS302); CSElectivePrereqs.Add(CS311); CSElectivePrereqs.Add(CS325);
            Course CSElective1 = new Course("CSElective1", "CSElective1", 4, true, new bool[] { true, true, false, true }, CSElectivePrereqs); schedule.courses.Add(CSElective1); schedule.ui_numberCredits += CSElective1.Credits;
            Course COMPUTING = new Course("COMPUTING", "COMPUTING", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(COMPUTING); schedule.ui_numberCredits += COMPUTING.Credits;

            // Spring 2021
            schedule = schedule.NextScheduleSimple();
            List<Course> CS380Prereqs = new List<Course>(); CS380Prereqs.Add(CS302);
            Course CS380 = new Course("CS380", "CS380", 4, true, new bool[] { false, true, false, false }, CS380Prereqs); schedule.courses.Add(CS380); schedule.ui_numberCredits += CS380.Credits;
            List<Course> CS420Prereqs = new List<Course>(); CS420Prereqs.Add(CS302); CS420Prereqs.Add(CS325); CS420Prereqs.Add(MATH330);
            Course CS420 = new Course("CS420", "CS420", 4, true, new bool[] { true, true, false, false }, CS420Prereqs); schedule.courses.Add(CS420); schedule.ui_numberCredits += CS420.Credits;
            Course CSElective2 = new Course("CSElective2", "CSElective2", 4, true, new bool[] { false, false, false, false }, CSElectivePrereqs); schedule.courses.Add(CSElective2); schedule.ui_numberCredits += CSElective2.Credits;
            Course BREADTH7 = new Course("BREADTH7", "BREADTH7", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH7); schedule.ui_numberCredits += BREADTH7.Credits;

            // Summer 2021
            schedule = schedule.NextScheduleSimple();
            schedule.locked = true;

            // Fall 2021
            schedule = schedule.NextScheduleSimple();
            List<Course> CS480Prereqs = new List<Course>(); CS480Prereqs.Add(CS325); CS480Prereqs.Add(CS380);
            Course CS480 = new Course("CS480", "CS480", 4, true, new bool[] { false, false, false, true }, CS480Prereqs); schedule.courses.Add(CS480); schedule.ui_numberCredits += CS480.Credits;
            List<Course> CS427Prereqs = new List<Course>(); CS427Prereqs.Add(CS302); CS427Prereqs.Add(CS325); CS427Prereqs.Add(MATH330);
            Course CS427 = new Course("CS427", "CS427", 4, true, new bool[] { true, false, false, true }, NoPrereqs); schedule.courses.Add(CS427); schedule.ui_numberCredits += CS427.Credits;
            Course UnivElective1 = new Course("UnivElective1", "UnivElective1", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(UnivElective1); schedule.ui_numberCredits += UnivElective1.Credits;
            List<Course> CS392Prereqs = new List<Course>(); CS392Prereqs.Add(CS302); CS392Prereqs.Add(CS325); CS392Prereqs.Add(CS312);
            Course CS392 = new Course("CS392", "CS392", 1, true, new bool[] { true, true, false, true }, CS392Prereqs); schedule.courses.Add(CS392); schedule.ui_numberCredits += CS392.Credits;

            // Winter 2022
            schedule = schedule.NextScheduleSimple();
            List<Course> CS481Prereqs = new List<Course>(); CS481Prereqs.Add(CS480);
            Course CS481 = new Course("CS481", "CS481", 4, true, new bool[] { true, false, false, false }, CS481Prereqs); schedule.courses.Add(CS481); schedule.ui_numberCredits += CS481.Credits;
            Course BREADTH8 = new Course("BREADTH8", "BREADTH8", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH8); schedule.ui_numberCredits += BREADTH8.Credits;
            Course CSElective3 = new Course("CSElective3", "CSElective3", 4, true, new bool[] { true, true, false, true }, CSElectivePrereqs); schedule.courses.Add(CSElective3); schedule.ui_numberCredits += CSElective3.Credits;
            Course UnivElective2 = new Course("UnivElective2", "UnivElective2", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(UnivElective2); schedule.ui_numberCredits += UnivElective2.Credits;

            // Spring 2022
            schedule = schedule.NextScheduleSimple();
            List<Course> CS489Prereqs = new List<Course>(); CS489Prereqs.Add(CS392);
            Course CS489 = new Course("CS489", "CS489", 1, true, new bool[] { true, true, false, true }, CS489Prereqs); schedule.courses.Add(CS489); schedule.ui_numberCredits += CS489.Credits;
            List<Course> CS492Prereqs = new List<Course>(); CS492Prereqs.Add(CS392);
            Course CS492 = new Course("CS492", "CS492", 2, true, new bool[] { true, true, false, true }, CS492Prereqs); schedule.courses.Add(CS492); schedule.ui_numberCredits += CS492.Credits;
            Course BREADTH9 = new Course("BREADTH9", "BREADTH9", 5, true, new bool[] { true, true, true, true }, NoPrereqs); schedule.courses.Add(BREADTH9); schedule.ui_numberCredits += BREADTH9.Credits;

            // Iterate back to the first quarter in the schedule
            while (schedule.previousQuarter != null)
            {
                schedule = schedule.previousQuarter;
            }

            List<Course> UnmetRequirements = new List<Course>();
            Course CSElective4 = new Course("CSElective4", "CSElective4", 4, true, new bool[] { true, true, false, true }, CSElectivePrereqs); UnmetRequirements.Add(CSElective4);
            Course CSElective5 = new Course("CSElective5", "CSElective5", 4, true, new bool[] { true, true, false, true }, CSElectivePrereqs); UnmetRequirements.Add(CSElective5);

            Schedule CompletedSchedule = Algorithm.Generate(UnmetRequirements, schedule, 0, 18, false);

            // Iterate back to the first quarter in the schedule
            while (CompletedSchedule.previousQuarter != null)
            {
                CompletedSchedule = CompletedSchedule.previousQuarter;
            }

            Schedule CurrentSchedule = CompletedSchedule;
            while (CompletedSchedule != null)
            {
                CurrentSchedule = CompletedSchedule;
                Console.WriteLine(CompletedSchedule.quarterName);
                foreach (Course course in CompletedSchedule.courses)
                {
                    Console.WriteLine("\t" + course.ID);
                }
                CompletedSchedule = CompletedSchedule.NextQuarter;
            }
            Console.Read();
        }
    }
}