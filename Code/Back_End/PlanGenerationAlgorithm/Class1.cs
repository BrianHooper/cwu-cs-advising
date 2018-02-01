using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;

namespace PlanGenerationAlgorithm
{
    public class Class1
    {
        public static void Main()
        {

            //fields
            Name n_name; //student name
            string s_ID = "3829"; //student ID
            uint quarter = 2018; //starting quarter
            Season s = Season.Fall;
            uint ui_minQuartersAtCWU = new uint(); //min quarter
            uint ui_minElectiveCredits = new uint(); //min elective
            uint ui_numberCredits = new uint();  //number credits
            Quarter q_startingQuarter = new Quarter(quarter, s); //starting quarter
            double d_minCumulativeGPA = 3.0;
            bool b_requiresMajor = false;
            int minCredits, maxCredits;
            CatalogCreditRequirements ccr_creditRequirements = new CatalogCreditRequirements();
            List<DegreeRequirements> l_degreeRequirements = new List<DegreeRequirements>();
            string s_name = "asd";
            Student student1 = new Student(n_name, s_ID, q_startingQuarter);
            Student student2 = new Student(n_name, "350", q_startingQuarter);
            Student student3 = new Student(n_name, "325", q_startingQuarter);
            CatalogRequirements catalog = new CatalogRequirements(s_ID, ui_minQuartersAtCWU, d_minCumulativeGPA, ccr_creditRequirements,
                                         l_degreeRequirements);
            Course course = new Course(s_name, s_ID, ui_numberCredits, b_requiresMajor);
            Course course2 = new Course(s_name, s_ID, ui_numberCredits, b_requiresMajor);
            Course course3 = new Course(s_name, s_ID, ui_numberCredits, b_requiresMajor);
            List<Course> courses = new List<Course>();
            DegreeRequirements deg1 = new DegreeRequirements(courses, courses, courses, courses,
                                   ui_minElectiveCredits, d_minCumulativeGPA, s_name);
            deg1.GeneralRequirements.Append(course);
            deg1.GeneralRequirements.Append(course2);
            deg1.GeneralRequirements.Append(course3);
            catalog.DegreeRequirements.Append(deg1);
            courses.Add(course);
            courses.Add(course2);
            courses.Add(course3);


            //catalog.DegreeRequirement;
            List<Schedule> schedules = new List<Schedule>();
            for (int i = 0; i < 12; i++)
            {
                schedules.Add(new Schedule(q_startingQuarter, ui_numberCredits, courses));
            }
            List<Student> students = new List<Student>();

            l_degreeRequirements.Add(new DegreeRequirements(courses, courses, courses, courses,
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
                        courses.Add(course1);
                    }
                }
            }


        }
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
    }
}
