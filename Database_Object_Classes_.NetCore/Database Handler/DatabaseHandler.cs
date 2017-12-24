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
            /*
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
            */
            
            RetrieveStudentPlan("12345678");


            MySqlConnection test = GetDBConnection();
            test.Open();

            string query = GetInsertQuery("test_db", "student_plans", "\"42345678\", 1, \"Winter14\", \"CS101,CS110,GE1,UNIV101\"");

            MySqlCommand testQuery = new MySqlCommand(query, test);

            testQuery.ExecuteNonQuery();

            test.Close();

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

        private static string GetInsertQuery(string s_db, string s_table, string s_values)
        {
            //"INSERT INTO test_db.student_plans\nVALUES(\"22345678\", 1, \"Winter14\", \"CS101,CS110,GE1,UNIV101\")"

            string query = "INSERT INTO ";
            query += s_db;
            query += ".";
            query += s_table;
            query += "\n VALUES(";
            query += s_values;
            query += ")";

            return query;
        }

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

        static object RetrieveStudentPlan(string s_ID)
        {
            // Variables:
            string[] data = null;

            MySqlConnection conn = GetDBConnection();

            MySqlDataReader reader;

            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "SELECT * FROM test_db.student_plans WHERE SID = \"" + s_ID + "\"",
                Connection = conn
            };

            conn.Open();

            reader = cmd.ExecuteReader();

            

            if (reader.HasRows)
            {
                // Variables:
                int i_offset = 2; // offset for the SID and WP columns
                int i_fields = reader.FieldCount - i_offset; // number of rows in actual plan
                int WP = -1;

                data = new string[i_fields];

                reader.Read();

                WP = (int)reader.GetValue(1);

                
                for(int i = 0; i < i_fields; i++)
                {
                    data[i] = (string)reader.GetValue(i + i_offset);
                } // end while
            } // end if

            conn.Close();

            return data;
        } // end method RetrieveStudentPlan

        static object RetrieveUserCredentials(string s_ID)
        {
            /// TODO
            return null;
        }

        private static MySqlConnection GetDBConnection()
        {
            // Variables:
            string          s_connStr  = "server=localhost;port=3306;database=test_db;user id=testuser;password=abc123;";
            //s_connStr = "server=<TBD>;user id=<TBD>;password=<TBD>;persistsecurityinfo=True;port=<TBD>;database=<DB>";
            MySqlConnection connection = new MySqlConnection(s_connStr);


            return connection;
        } // end method GetDBConnection
    } // end Class DatabaseHandler
} // end namespace Database_Handler
