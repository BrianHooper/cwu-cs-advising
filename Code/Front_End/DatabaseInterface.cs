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
        /// <summary>Set to true if the client connected to the database sucessfully</summary>
        public bool connected = false;

        private readonly TcpClient tcpClient;

        private readonly int tcpPort;

        private readonly NetworkStream networkStream;

        private const int BUFFER_SIZE = 2048;

        /// <summary>Extracts settings from configuration file.</summary>
        /// <param name="s_fileName">Ini file with configurations.</param>
        public DatabaseInterface(string s_fileName)
        {
            if (!connected)
            {
                try
                {
                    string ip = "127.0.0.1";
                    tcpPort = 44765;


                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), tcpPort);
                    tcpClient = new TcpClient();
                    tcpClient.Connect(endPoint);
                    networkStream = tcpClient.GetStream();
                    connected = true;
                }
                catch (IniParser.Exceptions.ParsingException e)
                {
                    Program.DbError = "ParsingException";
                    WriteToLog("Error reading configuration file. Msg: {0} " + e.Message);
                }
                catch (ArgumentNullException e)
                {
                    WriteToLog("DatabaseInterface constructor threw exception: " + e.Message);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    WriteToLog("DatabaseInterface constructor threw exception: " + e.Message);
                }
                catch(Exception e)
                {
                    WriteToLog("DatabaseInterface constructor threw exception: " + e.Message);
                }
            }
            
        }

        /// <summary>Default destructor, terminates database connection.</summary>
        ~DatabaseInterface()
        {
            DatabaseCommand disconnect = new DatabaseCommand(CommandType.Disconnect);

            SendCommand(disconnect);
        } // end Destructor

        /// <summary>Sends the given command to the database.</summary>
        /// <param name="cmd">The command to be sent.</param>
        private void SendCommand(DatabaseCommand cmd)
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            if (cmd.OperandType == OperandType.Course)
            {
                WriteToLog("SendCommand: Course Department: " + ((Course)cmd.Operand).Department);
                foreach(Course prereq in ((Course)cmd.Operand).PreRequisites)
                {
                    WriteToLog("SendCommand: Course Prereq: " + prereq);
                }
            }

            try
            {
                binaryFormatter.Serialize(memoryStream, cmd);
                byte[] data = memoryStream.ToArray();

                while (!networkStream.CanWrite) ;

                networkStream.Write(data, 0, data.Length);
            } // end try
            catch(Exception e)
            {
                WriteToLog("Error sending database command to Database. Msg: {0} " + e.Message);
            } // end catch

            memoryStream.Dispose();
        } // end method sendCommand

        /// <summary>Awaits a new command from the database.</summary>
        /// <returns>The command received from the database.</returns>
        private DatabaseCommand ReceiveCommand()
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            memoryStream = new MemoryStream();

            byte[] ba_data = new byte[BUFFER_SIZE];
            while (!networkStream.DataAvailable) ;

            // read from stream in chunks of 2048 bytes
            for (int i = 0; networkStream.DataAvailable; i++)
            {
                if(networkStream.CanRead)
                {
                    int n = networkStream.Read(ba_data, 0, BUFFER_SIZE);
                    memoryStream.Write(ba_data, 0, n);
                }
            } // end for

            memoryStream.Position = 0;

            DatabaseCommand cmd = (DatabaseCommand)binaryFormatter.Deserialize(memoryStream);

            return cmd;
        } // end method ReceiveCommand

        /// <summary>Executes a login attempt.</summary>
        /// <returns>Whether or not the login was successful.</returns>
        /// <param name="cred">User credentials of the user attempting to login.</param>
        /// <remarks>The credentials object must contain the hashed password of the user.</remarks>
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
                WriteToLog("DatabaseInterface.Login returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method 


        /// <summary>Gets all courses stored in the database. Note: Passing false makes this method extremely slow, only use this if absolutely necessary.</summary>
        /// <param name="shallow">Whether or not a shallow list should be retrieved.</param>
        /// <returns>List of all courses stored in the database or null if not found.</returns>
        public List<Course> GetAllCourses(bool shallow)
        {
            DatabaseCommand databaseCommand = new DatabaseCommand(CommandType.DisplayCourses, shallow);


            SendCommand(databaseCommand);
            DatabaseCommand dbCommand = ReceiveCommand();

            if(dbCommand.ReturnCode == 0 && dbCommand.CommandType == CommandType.Return)
            {
                return dbCommand.CourseList;
            } // end if
            else
            {
                WriteToLog("DatabaseInterface.GetAllCourses returned error code: " + dbCommand.ErrorMessage);
                return new List<Course>();
            } // end else
        } // end method GetAllCourses

        /// <summary>Gets all catalogs stored in the database.</summary>
        /// <param name="shallow">Whether or not a shallow list should be retrieved.</param>
        /// <returns>List of all catalogs stored in the database or null if not found.</returns>
        public List<CatalogRequirements> GetAllCatalogs(bool shallow)
        {
            DatabaseCommand databaseCommand = new DatabaseCommand(CommandType.DisplayCatalogs, shallow);

            SendCommand(databaseCommand);
            DatabaseCommand dbCommand = ReceiveCommand();

            if (dbCommand.ReturnCode == 0 && dbCommand.CommandType == CommandType.Return)
            {
                return dbCommand.CatalogList;
            } // end if
            else
            {
                WriteToLog("GetAllCatalogs returned error code: " + dbCommand.ErrorMessage);
                return new List<CatalogRequirements>();
            } // end else
        } // end method GetAllCatalogs


        /// <summary>Retrieves all users stored in the database.</summary>
        /// <returns>A list of all users.</returns>
        public List<Credentials> GetAllUsers()
        {
            DatabaseCommand databaseCommand = new DatabaseCommand(CommandType.DisplayUsers);

            SendCommand(databaseCommand);
            DatabaseCommand dbCommand = ReceiveCommand();

            if (dbCommand.ReturnCode == 0 && dbCommand.CommandType == CommandType.Return)
            {
                return dbCommand.UserList;
            } // end if
            else
            {
                WriteToLog("DatabaseInterface.GetAllUsers returned error code: " + dbCommand.ErrorMessage);
                return new List<Credentials>();
            } // end else
        } // end method GetAllUsers



        /// <summary>Retrieves a record from the database.</summary>
        /// <returns>The requested record or null if not found.</returns>
        /// <param name="template">Template containing the key of the object to retrieve.</param>
        /// <param name="ot_type">The type of object passed in arg 1.</param>
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
                WriteToLog("DatabaseInterface.RetrieveRecord returned error code: " + retCmd.ErrorMessage);
                return null;
            } // end else
        } // end method RetrieveRecord

        /// <summary>Retrieves a record from the database.</summary>
        /// <returns>The requested record or null if not found.</returns>
        /// <param name="template">Template containing the key of the object to retrieve.</param>
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
                WriteToLog("DatabaseInterface.RetrieveRecord returned error code: " + retCmd.ErrorMessage);
                return new Credentials();
            } // end else
        } // end method RetrieveRecord

        /// <summary>Retrieves a record from the database.</summary>
        /// <returns>The requested record or null if not found.</returns>
        /// <param name="template">Template containing the key of the object to retrieve.</param>
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
                WriteToLog("DatabaseInterface.RetrieveRecord returned error code: " + retCmd.ErrorMessage);
                return new PlanInfo();
            } // end else
        } // end method RetrieveRecord

        /// <summary>Retrieves the password salt from the website.</summary>
        /// <returns>A credentials object with the salt filled in.</returns>
        /// <param name="template">Template containing the key of the object to retrieve.</param>
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
                WriteToLog("DatabaseInterface.RetrieveSalt returned error code: " + retCmd.ErrorMessage);
                return new Credentials();
            } // end else
        } // end method RetrieveSalt


        /// <summary>Updates the record in the database</summary>
        /// <returns><c>true</c>, if record was updated, <c>false</c> otherwise.</returns>
        /// <param name="dbo">Object to update.</param>
        /// <param name="ot_type">Type of object in arg1.</param>
        public bool UpdateRecord(Database_Object dbo, OperandType ot_type)
        {
            WriteToLog("UpdateRecord: \n" + dbo);
            if(ot_type == OperandType.Course)
            {
                WriteToLog("UpdateRecord: Course Department: " + ((Course) dbo).Department);
            }
            else if(ot_type == OperandType.CatalogRequirements)
            {
                WriteToLog("UpdateRecord: CatalogRequirements " + ((CatalogRequirements)dbo).ID);
                foreach(DegreeRequirements degree in ((CatalogRequirements)dbo).DegreeRequirements)
                {
                    WriteToLog("UpdateRecord: DegreeRequirements " + degree.ID);
                }
            }
            DatabaseCommand cmd = new DatabaseCommand(CommandType.Update, dbo, ot_type);

            SendCommand(cmd);

            DatabaseCommand retCmd = ReceiveCommand();

            if(retCmd.CommandType == CommandType.Return && retCmd.ReturnCode == 0)
            {
                WriteToLog("UpdateRecord: return successful ");
                return true;
            } // end if
            else
            {
                WriteToLog("UpdateRecord invoked catch:" + retCmd.ErrorMessage);
                Pages.ManageCoursesModel.ErrorMessage = retCmd.ErrorMessage;
                return false;
            } // end else
        } // end method UpdateRecord


        /// <summary>Updates the record in the database</summary>
        /// <returns><c>true</c>, if record was updated, <c>false</c> otherwise.</returns>
        /// <param name="cred">Object to update.</param>
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
                WriteToLog("DatabaseInterface.UpdateRecord returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method UpdateRecord


        /// <summary>Updates the record in the database</summary>
        /// <returns><c>true</c>, if record was updated, <c>false</c> otherwise.</returns>
        /// <param name="info">Object to update.</param>
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
                WriteToLog("DatabaseInterface.UpdateRecord returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method UpdateRecord


        /// <summary>Updates the password of the specified user.</summary>
        /// <returns><c>true</c>, if password was updated, <c>false</c> otherwise.</returns>
        /// <param name="cred">Credentials object containing the new password hash.</param>
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
                WriteToLog("DatabaseInterface.UpdatePassword returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method UpdatePassword


        /// <summary>Deletes a record from the database.</summary>
        /// <returns><c>true</c>, if record was deleted, <c>false</c> otherwise.</returns>
        /// <param name="dbo">The object to delete.</param>
        /// <param name="ot_type">The type of object in arg1.</param>
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
                WriteToLog("DatabaseInterface.DeleteRecord returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method DeleteRecord


        /// <summary>Deletes a record from the database.</summary>
        /// <returns><c>true</c>, if record was deleted, <c>false</c> otherwise.</returns>
        /// <param name="cred">The object to delete.</param>
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
                WriteToLog("DatabaseInterface.DeleteRecord returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method DeleteRecord


        /// <summary>Deletes a record from the database.</summary>
        /// <returns><c>true</c>, if record was deleted, <c>false</c> otherwise.</returns>
        /// <param name="info">The object to delete.</param>
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
                WriteToLog("DatabaseInterface.DeleteRecord returned error code: " + retCmd.ErrorMessage);
                return false;
            } // end else
        } // end method DeleteRecord

        /// <summary>Writes to log file</summary>
        /// <param name="message">The error message to write</param>
        public static void WriteToLog(string message)
        {
            System.IO.File.AppendAllText("wwwroot/log.txt", DateTime.Now.ToLongTimeString() + "   --   " + message + "\n");
        }
    } // end Class DatabaseInterface
} // end Namespace CwuAdvising