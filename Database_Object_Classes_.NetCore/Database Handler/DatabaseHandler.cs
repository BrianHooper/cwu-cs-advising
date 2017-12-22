using System;
using MySql.Data.MySqlClient;
using Db4objects.Db4o;
using Database_Object_Classes;
using System.Collections.Generic;

namespace Database_Handler
{
    class DatabaseHandler
    {
        public static void Main(string[] args)
        {
            //Test();
            Student a = null;
            try
            {
                a = (Student)Retrieve("42345678", 'S');
            } // end try
            catch (RetrieveError e)
            {
                Console.WriteLine(e.Message + " The value received was: " + e.Type);
            } // end catch
            catch (Exception e)
            {
                Console.WriteLine("Unknown Exception occured: \n" + e.Message);
            } // end catch

            if (a == null)
            {
                Console.WriteLine("No such student.");
            } // end if
            else
            {
                Console.WriteLine("Student {0} has the ID {1}.", a.Name, a.ID);
                Console.WriteLine("This student's expected graduation is: {0}.", a.ExpectedGraduation.ToString());
                Console.WriteLine("This student has completed {0} credits.", a.CreditsCompleted);
                Console.WriteLine("This object's WP value is: {0}.", a.WP);
            } // end else
            


            Console.WriteLine("Hello World!");
        } // end Main

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
                    throw new RetrieveError("Invalid character received by Retrieve method.", c_type);
            } // end switch
        } // end method Retrieve


        private static Database_Object RetrieveHelper(string s_ID, char c_type)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        using (IObjectContainer db = Db4oFactory.OpenFile("Students.db4o"))
                        {
                                Database_Object student = db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                                db.Close();
                                return student;
                        } // end using
                    case 'Y':
                        using (IObjectContainer db = Db4oFactory.OpenFile("Catalogs.db4o"))
                        {
                                Database_Object catalog = db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                                db.Close();
                                return catalog;
                        } // end using
                    case 'C':
                        using (IObjectContainer db = Db4oFactory.OpenFile("Courses.db4o"))
                        {
                                Database_Object course = db.Query(delegate (Database_Object proto) { return proto.ID == s_ID; })[0];
                                db.Close();
                                return course;
                        } // end using
                } // end switch
            } // end try
            catch(Exception)
            {
                return null;
            } // end catch

            return null;
        } // end method RetrieveHelper

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
                db.Store(new Course("Computer Architecture 1", "CS311", 4, false));
                db.Store(new Course("Computer Architecture 2", "CS312", 4, true));
                db.Store(new Course("Intro to Software Engineering", "CS380", 4, true));
                db.Store(new Course("Software Engineering Project 1", "CS480", 4, true));
                db.Store(new Course("Software Engineering Project 2", "CS481", 4, true));
                db.Store(new Course("Intro to Programming 1", "CS111", 4, false));
                db.Store(new Course("Intro to Programming 2", "CS112", 4, false));

                db.Commit();

                db.Close();
            } // end using
        } // end method TestData

        static void RetrieveErrorType(char c_type)
        {
            /// TODO
        }
        static object RetrieveStudentPlan(string s_ID)
        {
            /// TODO
            return null;
        }
        static object RetrieveUserCredentials(string s_ID)
        {
            /// TODO
            return null;
        }

        private static MySqlConnection GetDBConnection(char c_type)
        {
            string s_connStr = "";
            switch(c_type)
            {
                case 'U': /// TODO
                    s_connStr = "server=<TBD>;user id=<TBD>;password=<TBD>;persistsecurityinfo=True;port=<TBD>;database=Credentials";
                    break;
                case 'P': /// TODO
                    s_connStr = "server=<TBD>;user id=<TBD>;password=<TBD>;persistsecurityinfo=True;port=<TBD>;database=StudentPlan";
                    break;
            } // end switch

            MySqlConnection connection = new MySqlConnection(s_connStr);

            connection.Open();

            return connection;
        } // end method GetDBConnection
    } // end Class DatabaseHandler
} // end namespace Database_Handler
