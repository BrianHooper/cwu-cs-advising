using Database_Object_Classes;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

namespace Database_Handler
{
    class Class1
    {
        public static void Main()
        {
            DatabaseHandler dbh = new DatabaseHandler();

            /*
            Course c = new Course("Computer Architecture 1", "CS311", 4, false);

            bool[] a = new bool[4] { true, false, true, false };

            c.SetQuarterOffered(a);
            c.AddPreRequisite(new Course("Calculus 1", "Math172", 5, false));

            /*/
            //*
            Student c = new Student(new Name("Al", "Bob"), "12345678", new Quarter(2014, Season.Fall), 47, 3.2, new AcademicStanding(false, true, true));

            c.ExpectedGraduation = new Quarter(2018, Season.Spring);
            c.GPA = 4.0;
            //*/

            Console.WriteLine("Object data in c:\n{0}", c.ToString());
            

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(ms, c);
                MemoryStream ms2 = new MemoryStream(ms.ToArray());
                BinaryFormatter format = new BinaryFormatter();
                //Course d = (Course)format.Deserialize(ms2);
                Student d = (Student)format.Deserialize(ms2);

                Console.WriteLine("\n\nObject data in d:\n{0}\n\n", d.ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine("Serialize failed, reason: {0}", e.Message);
            }           
            

            dbh.TestRun();
        }

    }
}
