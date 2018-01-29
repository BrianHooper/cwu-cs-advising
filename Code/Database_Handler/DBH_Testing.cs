using Database_Object_Classes;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Database_Handler
{
    class DBH_Testing
    {
        public static void Main()
        {
            DatabaseHandler dbh = new DatabaseHandler();
            dbh.SetUp();

            /*
            Course c = new Course("Computer Architecture 1", "CS311", 4, false);

            bool[] a = new bool[4] { true, false, true, false };

            c.SetQuarterOffered(a);
            c.AddPreRequisite(new Course("Calculus 1", "Math172", 5, false));

            DatabaseCommand cmd = new DatabaseCommand(CommandType.Retrieve, c, OperandType.Course);

            //*/
            //*
            Student c = new Student(new Name("Al", "Bob"), "12345678", new Quarter(2014, Season.Fall), 47, 3.2, new AcademicStanding(false, true, true))
            {
                ExpectedGraduation = new Quarter(2018, Season.Spring),
                GPA = 4.0
            };
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Retrieve, c, OperandType.Student);

            //*/
            //*
            Console.WriteLine("\n[Test Class]: Object data to be sent:\n{0}", c.ToString());




            MemoryStream ms = new MemoryStream();
            BinaryFormatter format = new BinaryFormatter();

            try
            {
                format.Serialize(ms, cmd);

            }
            catch(Exception e)
            {
                Console.WriteLine("Serialize failed, reason: {0}", e.Message);
            }           
            //*/

            ThreadStart method = new ThreadStart(dbh.TestRun);

            Thread thread = new Thread(method);
            thread.Start();

            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, 44765);

            socket.Connect(remoteEndPoint);

            byte[] data = ms.ToArray();

            int i = socket.Send(data);

            thread.Join();
        }

    }
}
