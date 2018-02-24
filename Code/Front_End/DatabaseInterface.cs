using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Net;
using Database_Object_Classes;
using Database_Handler;
using IniParser;
using IniParser.Model;

namespace CwuAdvising
{
    /// <summary></summary>
    public class DatabaseInterface
    {

        private readonly TcpClient tcpClient;

        private readonly int tcpPort;

        private readonly NetworkStream networkStream;

        private const int BUFFER_SIZE = 2048;

        /// <summary></summary>
        public DatabaseInterface()
        {
            string ipAddress = "127.0.0.1";
            tcpPort = 44765;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), tcpPort);
            tcpClient = new TcpClient(endPoint);
            networkStream = tcpClient.GetStream();
        }

        /// <summary>Extracts settings from configuration file.</summary>
        /// <param name="s_fileName">Ini file with configurations.</param>
        public DatabaseInterface(string s_fileName)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(s_fileName);

            string ip = data["Misc"]["IP"];
            string TCPPort = data["Misc"]["TCPIP_port"];
            tcpPort = int.Parse(TCPPort);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), tcpPort);
            tcpClient = new TcpClient(endPoint);
            networkStream = tcpClient.GetStream();
        }

        /// <summary></summary>
        ~DatabaseInterface()
        {
            DatabaseCommand disconnect = new DatabaseCommand(CommandType.Disconnect);

            SendCommand(disconnect);
        }


        private void SendCommand(DatabaseCommand cmd)
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            try
            {
                binaryFormatter.Serialize(memoryStream, cmd);
                networkStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
            } // end try
            catch(Exception e)
            {
                Console.Write("Error sending database command to Database. Msg: {0}", e.Message);
            } // end catch

            memoryStream.Dispose();
        } // end method sendCommand


        private DatabaseCommand ReceiveCommand()
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            memoryStream = new MemoryStream();

            byte[] ba_data = new byte[BUFFER_SIZE];

            // read from stream in chunks of 2048 bytes
            for (int i = 0; networkStream.DataAvailable; i++)
            {
                networkStream.Read(ba_data, i * BUFFER_SIZE, BUFFER_SIZE);
                memoryStream.Write(ba_data, i * BUFFER_SIZE, BUFFER_SIZE);
            } // end for

            DatabaseCommand cmd = (DatabaseCommand)binaryFormatter.Deserialize(memoryStream);

            return cmd;
        } // end method ReceiveCommand


        public bool Login(Credentials cred)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Login, cred);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        /// <summary></summary>
        public List<Course> GetAllCourses()
        {
            DatabaseCommand databaseCommand = new DatabaseCommand(CommandType.DisplayCourses);


            SendCommand(databaseCommand);
            DatabaseCommand dbCommand = ReceiveCommand();


            if(dbCommand.ReturnCode == 0 && dbCommand.CommandType == CommandType.Return)
            {
                return dbCommand.CourseList;
            } // end if
            else
            {
                return null;
            } // end else
        }


        public Database_Object RetrieveRecord(Database_Object template, OperandType ot_type)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Retrieve, template, ot_type);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return (Database_Object)retCmd.Operand;
            } // end if
            else
            {
                return null;
            } // end else
        }

        public Credentials RetrieveRecord(Credentials template)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Retrieve, template);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return (Credentials)retCmd.Operand;
            } // end if
            else
            {
                return new Credentials();
            } // end else
        }


        public PlanInfo RetrieveRecord(PlanInfo template)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Retrieve, template);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return (PlanInfo)retCmd.Operand;
            } // end if
            else
            {
                return new PlanInfo();
            } // end else
        }


        public Credentials RetrieveSalt(Credentials template)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.GetSalt, template);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return (Credentials)retCmd.Operand;
            } // end if
            else
            {
                return new Credentials();
            } // end else
        }


        public bool UpdateRecord(Database_Object dbo, OperandType ot_type)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Update, dbo, ot_type);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if(retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        public bool UpdateRecord(Credentials cred)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Update, cred);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        public bool UpdateRecord(PlanInfo info)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Update, info);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        public bool UpdatePassword(Credentials cred)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.ChangePW, cred);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }

        public bool DeleteRecord(Database_Object dbo, OperandType ot_type)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Delete, dbo, ot_type);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        public bool DeleteRecord(Credentials cred)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Delete, cred);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }


        public bool DeleteRecord(PlanInfo info)
        {
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Delete, info);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if (retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                return true;
            } // end if
            else
            {
                return false;
            } // end else
        }

    }
}
