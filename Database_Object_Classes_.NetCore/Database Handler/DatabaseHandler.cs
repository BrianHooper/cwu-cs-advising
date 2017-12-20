using System;
using Db4objects.Db4o;
using Database_Object_Classes;
using System.Collections.Generic;

namespace Database_Handler
{
    class DatabaseHandler
    {
        static void Main(string[] args)
        {
            //Test();
            Student a = (Student)Retrieve("52345678", 'S');
            if(a == null)
            {
                Console.WriteLine("No such student.");
            }
            else
            {
                Console.WriteLine("Student {0} has the ID 52345678", a.Name);
            }
            


            Console.WriteLine("Hello World!");
        }

        private static object Retrieve(string s_ID, char c_type)
        {
            switch (c_type)
            {
                case 'S':
                case 'Y':
                case 'C':
                    return RetrieveHelper(s_ID, c_type);
                case 'U':
                    return RetrieveUserCredentials(s_ID);
                case 'P':
                    return RetrieveStudentPlan(s_ID);
                default:
                    RetrieveErrorType(c_type);
                    return null;
            }
        }


        private static Database_Object RetrieveHelper(string s_ID, char c_type)
        {
            switch (c_type)
            {
                case 'S':
                    IObjectContainer student_db = Db4oFactory.OpenFile("Students.db4o");
                    try
                    {
                        Database_Object student = student_db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                        student_db.Close();
                        return student;
                    }
                    catch (Exception)
                    {
                        student_db.Close();
                        return null;
                    }
                case 'Y':
                    IObjectContainer catalog_db = Db4oFactory.OpenFile("Catalogs.db4o");
                    try
                    {
                        Database_Object catalog = catalog_db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                        catalog_db.Close();
                        return catalog;
                    }
                    catch (Exception)
                    {
                        catalog_db.Close();
                        return null;
                    }
                case 'C':
                    IObjectContainer course_db = Db4oFactory.OpenFile("Courses.db4o");
                    try
                    { 
                        Database_Object course = course_db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                        course_db.Close();
                        return course;
                    }   
                    catch (Exception)
                    {
                        course_db.Close();
                        return null;
                    }
            }

            return null;
        }

        static void TestData()
        {
            using (IObjectContainer db = Db4oFactory.OpenFile("Students.db4o"))
            {
                db.Store(new Student(new Name("Al", "Gore"), "12345678", new Quarter(2014, Season.Fall), 0, 4.0, new AcademicStanding(false, false, true)));
                db.Store(new Student(new Name("Brian", "Adams"), "22345678", new Quarter(2010, Season.Spring), 65, 3.1, new AcademicStanding(false, false, true)));
                db.Store(new Student(new Name("Cal", "Brown"), "32345678", new Quarter(2011, Season.Fall), 90, 2.9, new AcademicStanding(false, true, true)));
                db.Store(new Student(new Name("Dan", "Power"), "42345678", new Quarter(2012, Season.Winter), 12, 3.4, new AcademicStanding(false, false, true)));

                db.Commit();

                db.Close();
            }

            using (IObjectContainer db = Db4oFactory.OpenFile("Catalogs.db4o"))
            {
                db.Store(new CatalogRequirements("Y2014", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2010", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2011", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));
                db.Store(new CatalogRequirements("Y2012", 3, 2.0, new CatalogCreditRequirements(), new List<DegreeRequirements>()));

                db.Commit();

                db.Close();
            }

            using (IObjectContainer db = Db4oFactory.OpenFile("Courses.db4o"))
            {
                db.Store(new Course("Computer Architecture 1", "CS311", 4, false));
                db.Store(new Course("Computer Architecture 2", "CS312", 4, true));
                db.Store(new Course("Intro to Software Engineering", "CS380", 4, true));
                db.Store(new Course("Software Engineering Project 1", "CS480", 4, true));
                db.Store(new Course("Software Engineering Project 2", "CS481", 4, true));
                db.Store(new Course("Intro to Programming 1", "CS111", 4, false));
                db.Store(new Course("Intro to Programming 2", "CS112", 4, false));

                db.Commit();

                db.Close();
            }
        }

        static void RetrieveErrorType(char c_type)
        {
            /// TODO
        }

        static object RetrieveStudentPlan(string s_ID)
        {
            return null;
        }
        static object RetrieveUserCredentials(string s_ID)
        {
            return null;
        }



    }
}
