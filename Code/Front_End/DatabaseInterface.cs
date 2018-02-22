using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Net;
using Database_Object_Classes;
using Database_Handler;

namespace CwuAdvising
{
    public class DatabaseInterface
    {

        private TcpClient tcpClient;

        private IPAddress ipAddress;

        private int tcpPort;

        private NetworkStream networkStream;

        public DatabaseInterface()
        {
            ipAddress = IPAddress.Any;
            tcpPort = 44765;

            IPEndPoint endPoint = new IPEndPoint(ipAddress, tcpPort);
            tcpClient = new TcpClient(endPoint);
            networkStream = tcpClient.GetStream();
        }

        ~DatabaseInterface()
        {
            DatabaseCommand disconnect = new DatabaseCommand(CommandType.Disconnect);

        }

        

        public List<Course> GetAllCourses()
        {
            DatabaseCommand databaseCommand = new DatabaseCommand(CommandType.DisplayCourses);

            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            try
            {
                binaryFormatter.Serialize(memoryStream, databaseCommand);
                networkStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
            } catch( Exception e)
            {
                Console.Write(e);
                return null;
            }

            byte[] ba_data = new byte[2048];
            networkStream.Read(ba_data, 0, 2048);

            memoryStream.Dispose();
            memoryStream = new MemoryStream(ba_data);

            DatabaseCommand dbCommand = (DatabaseCommand)binaryFormatter.Deserialize(memoryStream);

            if(dbCommand.ReturnCode == 0)
            {
                return dbCommand.CourseList;
            } else
            {
                return null;
            }
        }

    }
}
