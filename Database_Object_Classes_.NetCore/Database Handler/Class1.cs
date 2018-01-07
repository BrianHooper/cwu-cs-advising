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

            Course c = new Course("Computer Architecture 1", "CS311", 4, false);
            Student s = new Student(new Name("a", "b"), "123", new Quarter(1, Season.Fall));

            s.GPA = 4.0;

            bool[] a = new bool[4] { true, false, true, false };

            c.SetQuarterOffered(a);
            c.AddPreRequisite(new Course("Calculus 1", "Math172", 5, false));

            Console.WriteLine("Object data in c: {0}", c.ToString());


            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(ms, c);
                MemoryStream ms2 = new MemoryStream(ms.ToArray());
                BinaryFormatter format = new BinaryFormatter();
                Course d = (Course)format.Deserialize(ms2);
                //Student d = (Student)format.Deserialize(ms2);

                Console.WriteLine("Object data in d: {0}", c.ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine("Serialize failed, reason: {0}", e.Message);
            }           
            

            dbh.TestRun();
        }

    }
}
