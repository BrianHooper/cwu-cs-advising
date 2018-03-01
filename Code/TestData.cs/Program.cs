using System;
using Database_Object_Classes;
using Db4objects.Db4o;
using System.Collections.Generic;


namespace TestData.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            TestData();
            Console.Read();
        }


        /// <summary>Dummy records for testing.</summary>
        private static void TestData()
        {
            using (IObjectContainer db = Db4oFactory.OpenFile("Students.db4o"))
            {
                db.Store(new Student(new Name("Al", "Gore"), "12345678", new Quarter(2014, Season.Fall), 0, 4.0, new AcademicStanding(false, false, true)));
                db.Store(new Student(new Name("Brian", "Adams"), "22345678", new Quarter(2010, Season.Spring), 65, 3.1, new AcademicStanding(false, false, true)));
                db.Store(new Student(new Name("Cal", "Brown"), "32345678", new Quarter(2011, Season.Fall), 90, 2.9, new AcademicStanding(false, true, true)));
                db.Store(new Student(new Name("Dan", "Power"), "42345678", new Quarter(2012, Season.Winter), 12, 3.4, new AcademicStanding(false, false, true)));
                db.Store(new Student(new Name("James", "Bond"), "007", new Quarter(1962, Season.Fall), 84, 2.1, new AcademicStanding(false, true, true)));

                db.Commit();

                db.Close();
            } // end using

            using (IObjectContainer db = Db4oFactory.OpenFile("Catalogs.db4o"))
            {
                db.Store(new CatalogRequirements("Y2014", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2010", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2011", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2012", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));

                db.Commit();

                db.Close();
            } // end using

            using (IObjectContainer db = Db4oFactory.OpenFile("Courses.db4o"))
            {
                db.Store(new Course("Computer Architecture 1", "CS311", 4, false, new bool[4] { false, true, false, true }));
                db.Store(new Course("Computer Architecture 2", "CS312", 4, true, new bool[4] { true, false, false, true }));
                db.Store(new Course("Intro to Software Engineering", "CS380", 4, true, new bool[4] { false, true, false, false }));
                db.Store(new Course("Software Engineering Project 1", "CS480", 4, true, new bool[4] { false, false, false, true }));
                db.Store(new Course("Software Engineering Project 2", "CS481", 4, true, new bool[4] { true, false, false, false }));
                db.Store(new Course("Intro to Programming 1", "CS110", 4, false, new bool[4] { true, true, true, true }));
                db.Store(new Course("Intro to Programming 2", "CS111", 4, false, new bool[4] { true, true, true, true }));
                db.Store(new Course("Intro to Programming 2", "CS112", 4, false, new bool[4] { true, true, false, false }));
                db.Store(new Course("Operating Systems", "CS470", 4, false, new bool[4] { true, false, false, false }));
                db.Store(new Course("Principles of Language Design 1", "CS361", 4, false, new bool[4] { false, false, false, true }));
                db.Store(new Course("Principles of Language Design 2", "CS362", 4, false, new bool[4] { true, false, false, false }));
                db.Store(new Course("Algorithm Analysis", "CS427", 4, false, new bool[4] { true, false, false, true }));
                db.Store(new Course("Optimization", "CS471", 4, false, new bool[4] { false, true, false, false }));

                db.Commit();

                db.Close();
            } // end using
        } // end method TestData
    }
}
