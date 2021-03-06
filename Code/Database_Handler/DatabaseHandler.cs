﻿using System;
using MySql.Data.MySqlClient;
using Database_Object_Classes;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using IniParser;
using IniParser.Model;

namespace Database_Handler
{
    /// <summary>Database Handler is the middleman between the website and the databases. It retrieves, updates, creates, and deletes database entries.</summary>
    public sealed class DatabaseHandler : IDisposable
    {
        #region Static fields

        #region General

        /// <summary>The number of iterations for hashing the password.</summary>
        public static int i_HASH_ITERATIONS = 10000;
        /// <summary>The length of the salt being used for hashing user passwords.</summary>
        public static int i_SALT_LENGTH     = 32;
        /// <summary>The path to the log file which will contain the log entries created by DBH.</summary>
        public static string s_logFilePath = "/var/aspnetcore/logs/log.txt";

        #endregion

        #region Mutex Locks

        /// <summary>Mutex locks for databases.</summary>
        private static Mutex MySqlLock = new Mutex();
        private static Mutex LogLock = new Mutex();

        #endregion

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Class fields

        #region Readonly/Const

        #region MySQL

        #region MySQL Database Info
        private readonly string s_MYSQL_DB_NAME = "test_db";
        private readonly string s_MYSQL_DB_SERVER = "localhost";
        private readonly string s_MYSQL_DB_PORT = "3306";
        private readonly string s_MYSQL_DB_USER_ID = "testuser";
        #endregion

        #region MySQL Tables
        private readonly string s_CREDENTIALS_TABLE = "user_credentials";
        private readonly string s_STUDENTS_TABLE = "students";
        private readonly string s_CATALOGS_TABLE = "catalogs";
        private readonly string s_DEGREES_TABLE = "degrees";
        private readonly string s_COURSES_TABLE = "courses";
        private readonly string s_PLAN_TABLE = "student_plans";
        #endregion

        #region MySQL table keys
        private readonly string s_CREDENTIALS_KEY = "username";
        private readonly string s_STUDENTS_KEY = "SID";
        private readonly string s_CATALOGS_KEY = "catalog_year";
        private readonly string s_DEGREES_KEY = "degree_id";
        private readonly string s_COURSES_KEY = "course_id";
        private readonly string s_PLAN_KEY = "SID";
        #endregion

        #region MySQL Default Insert Queries
        private const string s_INSERT_COURSE_DEFAULT = "(course_id, course_name, offered_winter, offered_fall, offered_spring, offered_fall, num_credits, deparment)";
        private const string s_INSERT_PLAN_DEFAULT = "";
        private const string s_INSERT_DEGREE_DEFAULT = "";
        private const string s_INSERT_CATALOG_DEFAULT = "";
#endregion
        #endregion

        #region TCP info
        private readonly string s_IP_ADDRESS = "127.0.0.1";
        
        private readonly int i_TCP_PORT = 44765;
        #endregion

        #region General
        /// <summary>The buffer size for TCP packages.</summary>
        private const int  BUFFER_SIZE            = 2048;

        /// <summary>The maximum depth a recursive call my reach. At this point an exception is thrown to terminate the call.</summary>
        private const uint ui_MAX_RECURSION_DEPTH = 30;
        #endregion

        #endregion

        #region Privates

        #region MySQL Connection/Info
        /// <summary>The number of columns in each variable length table { s_PLAN_TABLE, s_COURSES_TABLE, s_CATALOGS_TABLE, s_DEGREES_TABLE}.</summary>
        private uint[] ui_COL_COUNT;

        private string[] s_TABLES;

        /// <summary>The master connection to the MySql database, which should never be closed, except during cleanup.</summary>
        private MySqlConnection DB_CONNECTION;
        #endregion

        #region General
        /// <summary>A RNG for building password salts.</summary>
        private RNGCryptoServiceProvider RNG;
        #endregion

        #region TCP Connection/Info
        /// <summary>The main Tcp Listener used to talk with clients.</summary>
        private TcpListener tcpListener;

        /// <summary>Localhost IP address for the Tcp Socket</summary>
        private IPAddress address;

        /// <summary>List of all client threads currently running.</summary>
        private List<Thread> clientThreads;

        /// <summary>Variable to alert the main thread that a deadlock occurred.</summary>
        private bool deadlocked = false;

        /// <summary>List of all clients currently connected to DBH.</summary>
        private List<TcpClient> clients;
        #endregion

        #endregion

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Constructor
        /// <summary>Loads the DBH configurations from the given .ini file.</summary>
        /// <param name="s_fileName">Path of the .ini file containing the configurations DBH should use.</param>
        public DatabaseHandler(string s_fileName)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(s_fileName);

            // Get MySQL database descriptors
            s_MYSQL_DB_NAME = data["MySql Connection"]["DB"];
            s_MYSQL_DB_SERVER = data["MySql Connection"]["host"];
            s_MYSQL_DB_PORT = data["MySql Connection"]["port"];
            s_MYSQL_DB_USER_ID = data["MySql Connection"]["user"];
            string pw = data["MySql Connection"]["password"];

            // Get MySQL table descriptors
            s_PLAN_TABLE = data["MySql Tables"]["grad_plans"];
            s_PLAN_KEY = data["MySql Tables"]["grad_plans_key"];

            s_CREDENTIALS_TABLE = data["MySql Tables"]["credentials"];
            s_CREDENTIALS_KEY = data["MySql Tables"]["credentials_key"];

            s_STUDENTS_TABLE = data["MySql Tables"]["students_table"];
            s_STUDENTS_KEY = data["MySql Tables"]["students_table_key"];

            s_CATALOGS_TABLE = data["MySql Tables"]["catalogs_table"];
            s_CATALOGS_KEY = data["MySql Tables"]["catalogs_table_key"];

            s_DEGREES_TABLE = data["MySql Tables"]["degrees_table"];
            s_DEGREES_KEY = data["MySql Tables"]["degrees_table_key"];

            s_COURSES_TABLE = data["MySql Tables"]["courses_table"];
            s_COURSES_KEY = data["MySql Tables"]["courses_table_key"];

            s_TABLES = new string[4];
            s_TABLES[0] = data["MySql Tables"]["grad_plans"];
            s_TABLES[1] = data["MySql Tables"]["courses_table"];
            s_TABLES[2] = data["MySql Tables"]["s_CATALOGS_TABLE"];
            s_TABLES[3] = data["MySql Tables"]["degrees_table"];
            
            // Get TCP/IP Settings
            s_IP_ADDRESS = data["Misc"]["IP"];
            s_logFilePath = data["Misc"]["logfile_path"];
            string TCPPort = data["Misc"]["TCPIP_port"];
            i_TCP_PORT = int.Parse(TCPPort);

            deadlocked = false;

            ConnectToDB(ref pw);
            SetUp(false);
        } // end Constructor

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Main
        /// <summary>Program entry point. Initializes program and handles fatal errors.</summary>
        /// <param name="args">Unused.</param>
        public static void Main(string[] args)
        {
            WriteToLog(" -- DBH was started.");

            DatabaseHandler DBH = new DatabaseHandler("/var/aspnetcore/publish/Configuration.ini");
            int i_errorCode = -1;

            try
            {
                i_errorCode = DBH.RunHost();

                switch (i_errorCode)
                {
                    case 0:
                        Console.WriteLine("\n\nNo errors occurred during execution.\nCleaning up...");
                        WriteToLog(" -- DBH is preparing to exit (no errors).");
                        break; // end case 0
                    case 2:
                        Console.WriteLine("\n\nThe connection to the database timed out and reconnection failed. Cleaning up...");
                        WriteToLog(" -- DBH is preparing to exit (error code 2).");
                        WriteToLog(" -- DBH is exiting because the MySQL database connection timed out and could not be reopened.");
                        break; // end case 2
                    case 3:
                        Console.WriteLine("\n\nThe DBH setup failed. Cleaning up...");
                        WriteToLog(" -- DBH is preparing to exit (error code 3).");
                        WriteToLog(" -- DBH is exiting because setup failed.");
                        break; // end case 3
                    case 74:
                        Console.WriteLine("\nDBH detected a potential deadlock and is restarting.");
                        WriteToLog(" -- DBH potential deadlock occurred.");
                        break;
                } // end switch
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH is preparing to exit (error code unknown).");
                WriteToLog(" -- DBH encountered an unknown exception. Msg: " + e.Message);
                i_errorCode = -1;
            } // end catch            
            finally
            {
                try
                {
                    WriteToLog(" -- DBH is cleaning up...");

                    DBH.Dispose();

                    WriteToLog(" -- DBH finished cleaning up and is exiting now.");
                    Console.WriteLine("Clean up finished, exiting.");
                } // end try
                catch (Exception e)
                {
                    WriteToLog(" -- DBH clean up was unsuccessfull.");
                    WriteToLog(" -- DBH encountered an exception while cleaning up. Msg: " + e.Message);
                } // end catch
                finally
                {
                    MySqlLock.Dispose();
                    LogLock.Dispose();
                } // end finally
            } // end finally 

            Environment.Exit(0); // let all other threads know main is exiting, and they should exit too
        } // end Main

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Run Methods
        /// <summary>The main program loop. Receives, and executes commands.</summary>
        /// <param name="stream">The network stream which is used to communicate with the client.</param>
        /// <returns>Error code or 0 if application exited as expected.</returns>
        private int Run(NetworkStream stream)
        {
            try
            {
                for (; ; )
                {
                    DatabaseCommand command = WaitForCommand(stream);
                    int i = ExecuteCommand(command, stream);

                    switch (i)
                    {
                        case 1:
                            WriteToLog(" -- DBH was not able to send return command due to an error.");
                            SendResult(new DatabaseCommand(1, "An error occurred while trying to send the requested data."), stream);
                            break;
                        case 99: // exit command
                            WriteToLog(" -- DBH client is terminating the connection.");
                            return 0;
                        default:
                            break;
                    } // end switch
                } // end for
            } // end try
            catch (ThreadAbortException)
            {
                WriteToLog(" -- DBH Thread received kill signal. Exiting ...");
                return 0;
            } // end catch
            catch (Exception e)
            {
                if (!int.TryParse(e.Message, out int error))
                {
                    throw e; // not an exception generated by DBH
                } // end if      

                return error;
            } // end catch
        } // end method Run

        /// <summary>Client thread entry point.</summary>
        /// <param name="client">The client this thread is responsible for.</param>
        private void RunClient(Object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            int exitCode = 1;

            try
            {
                exitCode = Run(clientStream);
            } // end try
            catch (ThreadAbortException)
            {
                WriteToLog(" -- DBH client thread received abort signal. Exiting ...");
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH run caused an exception: " + e.Message);
            } // end catch
            finally
            {
                switch (exitCode)
                {
                    case 0:
                        WriteToLog(" -- DBH Client connection successfully terminated.");
                        break;
                    default:
                        WriteToLog(" -- DBH An error occurred during execution of client thread.");
                        break;
                } // end switch

                tcpClient.Close();
            } // end finally
        } // end method RunClient

        /// <summary>Host thread handling all client threads, and managing new connections.</summary>
        /// <returns>Never returns in normal operation, returns error code if a fatal error occurs.</returns>
        private int RunHost()
        {
            var output = 0;

            Thread stayAlive = new Thread(KeepAlive);
            stayAlive.Start();

            Thread deadlockThread = new Thread(DeadLockDetector);
            deadlockThread.Start();

            for (; ; )
            {
                // close old connections while waiting for new ones to arrive
                while(!tcpListener.Pending())
                {
                    try
                    {
                        for(int i = 0; i < clients.Count; i++)
                        {
                            if (!clients[i].Connected)
                            {
                                clients[i].Close();
                            } // end if
                            if(tcpListener.Pending())
                            {
                                break;
                            } // end if
                        } // end for
                    } // end try
                    catch(Exception e)
                    {
                        WriteToLog(" -- DBH an exception occurred while checking old connections. Msg: " + e.Message);
                    } // end catch
                } // end while

                TcpClient newClient = tcpListener.AcceptTcpClient();

                if (newClient == null)
                {
                    WriteToLog(" -- DBH The client object was null.");
                    continue;
                } // end if

                Thread newClientThread = new Thread(RunClient);

                // if the session expires, attempt to reopen it
                if (DB_CONNECTION.State != System.Data.ConnectionState.Open)
                {
                    try
                    {
                        AttemptReconnect();
                    } // end try
                    catch (Exception)
                    {
                        output = 2;

                        WriteToLog(" -- DBH lost the database connection and is terminating all client threads.");
                        foreach (Thread t in clientThreads)
                        {
                            t.Abort();
                        } // end foreach
                        foreach (TcpClient c in clients)
                        {
                            c.Close();
                        } // end foreach

                        stayAlive.Abort();
                        deadlockThread.Abort();
                        break;
                    } // end catch
                } // end if

                clients.Add(newClient);
                clientThreads.Add(newClientThread);
                newClientThread.Start(newClient);
            } // end for

            return output;
        } // end method RunHost

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Command methods
        /// <summary>Unwraps the command type, and executes the specified command.</summary>
        /// <param name="cmd">The command to be executed.</param>
        /// <param name="stream">The stream to send data back to the client.</param>
        /// <returns>An error code, or 0 if execution was successful.</returns>
        private int ExecuteCommand(DatabaseCommand cmd, NetworkStream stream)
        {
            DatabaseCommand output = null;

            try
            {
                switch (cmd.CommandType)
                {
                    case CommandType.Retrieve:
                        output = ExecuteRetrieveCommand(cmd);
                        break;
                    case CommandType.Update:
                        output = ExecuteUpdateCommand(cmd);
                        break;
                    case CommandType.Delete:
                        output = ExecuteDeleteCommand(cmd);
                        break;
                    case CommandType.ChangePW:
                        output = ExecutePasswordChangeCommand(cmd);
                        break;
                    case CommandType.Login:
                        output = ExecuteLoginCommand(cmd);
                        break;
                    case CommandType.DisplayCatalogs:
                        output = ExecuteDisplayCatalogsCommand(cmd.IsShallow);
                        break;
                    case CommandType.DisplayUsers:
                        output = ExecuteDisplayUsersCommand();
                        break;
                    case CommandType.GetSalt:
                        output = ExecuteGetSaltCommand(cmd);
                        break;
                    case CommandType.DisplayCourses:
                        output = ExecuteDisplayCoursesCommand();
                        if (!cmd.IsShallow)
                        {
                            foreach (Course c in output.CourseList)
                            {
                                foreach (string s in c.ShallowPreRequisites)
                                {
                                    c.AddPreRequisite(RetrieveCourse(s, false, 0));
                                } // end foreach
                            } // end foreach
                        } // end if
                        break;
                    case CommandType.Disconnect:
                        output = new DatabaseCommand(99, "Exit command received.");
                        break;
                    default:
                        output = new DatabaseCommand(-1, "Invalid command type");
                        break;
                } // end switch
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception reached Execute Command. Msg: " + e.Message);
            } // end catch

            return SendResult(output, stream);
        } // end method ExecuteCommand

        #region Execute Commands
        /// <summary>Executes a Retrieve command.</summary>
        /// <param name="cmd">Command containing object to be retrieved.</param>
        /// <returns>A return command containing information about execution success, and, if successful, the requested data.</returns>
        private DatabaseCommand ExecuteRetrieveCommand(DatabaseCommand cmd)
        {
            Database_Object dbo;
            Credentials cred;
            PlanInfo plan;
            DatabaseCommand result = null;

            try
            {
                switch (cmd.OperandType)
                {
                    case OperandType.CatalogRequirements:
                        dbo = (CatalogRequirements)cmd.Operand;
                        result = new DatabaseCommand(CommandType.Return, (CatalogRequirements)Retrieve(dbo.ID, 'Y', cmd.IsShallow), OperandType.CatalogRequirements);
                        break;
                    case OperandType.Student:
                        dbo = (Student)cmd.Operand;
                        result = new DatabaseCommand(CommandType.Return, (Student)Retrieve(dbo.ID, 'S'), OperandType.Student);
                        break;
                    case OperandType.Course:
                        dbo = (Course)cmd.Operand;
                        Course temp = (Course)Retrieve(dbo.ID, 'C', cmd.IsShallow);
                        result = new DatabaseCommand(CommandType.Return, temp, OperandType.Course);
                        break;
                    case OperandType.Credentials:
                        cred = (Credentials)cmd.Operand;
                        result = new DatabaseCommand(CommandType.Return, (Credentials)Retrieve(cred.UserName, 'U'));
                        break;
                    case OperandType.PlanInfo:
                        plan = (PlanInfo)cmd.Operand;
                        result = new DatabaseCommand(CommandType.Return, (PlanInfo)Retrieve(plan.StudentID, 'P'));
                        break;
                    default:
                        result = new DatabaseCommand(-1, "Invalid operand type");
                        break;
                } // end switch
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception made it to Execute Retrieve Command! Msg: " + e.Message);
            } // end catch

            return result;
        } // end method ExecuteRetrieveCommand

        /// <summary>Executes an Update command.</summary>
        /// <param name="cmd">Command containing object to be updated.</param>
        /// <returns>A return command containing information about execution success.</returns>
        private DatabaseCommand ExecuteUpdateCommand(DatabaseCommand cmd)
        {
            Database_Object dbo;
            Credentials cred;
            PlanInfo plan;
            int i_code = -2;
            string s_msg = "Update failed";

            GetColumnCounts("", 0, true);

            try
            {
                switch (cmd.OperandType)
                {
                    case OperandType.CatalogRequirements:
                        dbo = (CatalogRequirements)cmd.Operand;
                        i_code = Update((CatalogRequirements)dbo);
                        break;
                    case OperandType.Student:
                        dbo = (Student)cmd.Operand;
                        i_code = Update((Student)dbo);
                        break;
                    case OperandType.Course:
                        dbo = (Course)cmd.Operand;
                        i_code = Update((Course)dbo);
                        break;
                    case OperandType.Credentials:
                        cred = (Credentials)cmd.Operand;
                        i_code = Update(cred);
                        break;
                    case OperandType.PlanInfo:
                        plan = (PlanInfo)cmd.Operand;
                        i_code = Update(plan);
                        break;
                    default:
                        i_code = -1;
                        break;
                } // end switch
            }
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception made it to Execute Update Command! Msg: " + e.Message);
            } // end catch

            if (i_code == 0)
            {
                return new DatabaseCommand(i_code);
            } // end if
            else
            {
                return new DatabaseCommand(i_code, s_msg);
            } // end else            
        } // end method ExecuteRetrieveCommand

        /// <summary>Executes a Delete command.</summary>
        /// <param name="cmd">Command containing object to be deleted.</param>
        /// <returns>A return command containing information about execution success.</returns>
        private DatabaseCommand ExecuteDeleteCommand(DatabaseCommand cmd)
        {
            Database_Object dbo;
            Credentials cred;
            PlanInfo plan;
            int i_code = -2;

            try
            {
                switch (cmd.OperandType)
                {
                    case OperandType.CatalogRequirements:
                        dbo = (CatalogRequirements)cmd.Operand;
                        i_code = DeleteRecord((CatalogRequirements)dbo);
                        break;
                    case OperandType.Student:
                        dbo = (Student)cmd.Operand;
                        i_code = DeleteRecord((Student)dbo);
                        break;
                    case OperandType.Course:
                        dbo = (Course)cmd.Operand;
                        i_code = DeleteRecord((Course)dbo);
                        break;
                    case OperandType.Credentials:
                        cred = (Credentials)cmd.Operand;
                        i_code = DeleteRecord(cred);
                        break;
                    case OperandType.PlanInfo:
                        plan = (PlanInfo)cmd.Operand;
                        i_code = DeleteRecord(plan);
                        break;
                    default:
                        i_code = -1;
                        break;
                } // end switch
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception made it to Execute Delete Command! Msg: " + e.Message);
            } // end catch

            if (i_code == 0)
            {
                return new DatabaseCommand(i_code);
            } // end if
            else
            {
                return new DatabaseCommand(i_code, "Delete Failed");
            } // end else            
        } // end method ExecuteRetrieveCommand

        /// <summary>Executes a login command.</summary>
        /// <param name="cmd">Command containing credentials of user trying to login.</param>
        /// <returns>A return command containing information about execution success.</returns>
        private DatabaseCommand ExecuteLoginCommand(DatabaseCommand cmd)
        {
            DatabaseCommand output;
            Credentials cred = (Credentials)cmd.Operand;
            bool b_success = false;

            try
            {
                string s_pw = BitConverter.ToString(cred.Password_Hash);
                s_pw = s_pw.Replace("-", string.Empty);

                b_success = LoginAttempt(cred.UserName, s_pw);
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception made it to Execute Login Command! Msg: " + e.Message);
            } // end catch

            if (b_success)
            {
                output = new DatabaseCommand();
            } // end if
            else
            {
                output = new DatabaseCommand(1, "Login failed, username/password incorrect.");
            } // end else

            return output;
        } // end method ExecuteLoginCommand

        /// <summary>Executes a password change.</summary>
        /// <param name="cmd">Command containing credentials of user trying to change password.</param>
        /// <returns>A return command containing information about execution success.</returns>
        private DatabaseCommand ExecutePasswordChangeCommand(DatabaseCommand cmd)
        {
            DatabaseCommand output;
            Credentials cred = (Credentials)cmd.Operand;

            int i_errorCode = -2;

            try
            {
                string s_pw = BitConverter.ToString(cred.Password_Hash);
                s_pw = s_pw.Replace("-", "");
                i_errorCode = ChangePassword(cred.UserName, s_pw, true);
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH an exception made it to Execute Password Change Command! Msg: " + e.Message);
            } // end catch

            if (i_errorCode == 0)
            {
                output = new DatabaseCommand();
            } // end if
            else
            {
                output = new DatabaseCommand(i_errorCode, "Password change failed, user does not exist.");
            } // end else

            return output;
        } // end method ExecutePasswordChangeCommand

        /// <summary>Executes a display catalogs command.</summary>
        /// <param name="b_shallow">Whether or not to retrieve a shallow catalog object.</param>
        /// <returns>A return command containing a list of all catalogs in the database.</returns>
        private DatabaseCommand ExecuteDisplayCatalogsCommand(bool b_shallow)
        {
            List<CatalogRequirements> catalogs = new List<CatalogRequirements>();
            MySqlDataReader reader;

            try
            {
                MySqlLock.WaitOne();
                string query = "SELECT * FROM " + s_MYSQL_DB_NAME + "." + s_CATALOGS_TABLE + ";";

                MySqlCommand cmd = new MySqlCommand(query, DB_CONNECTION);
                reader = cmd.ExecuteReader();

                List<string> l_IDs = new List<string>();

                while (reader.Read())
                {
                    string s_catalog = reader.GetString(0);

                    l_IDs.Add(s_catalog);
                } // end while

                reader.Close();
                MySqlLock.ReleaseMutex();

                foreach(string s in l_IDs)
                {
                    catalogs.Add(RetrieveCatalog(s, b_shallow));
                } // end foreach
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH display catalogs command failed. Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving list of all catalogs failed. Msg: " + e.Message);
            } // end catch

            return new DatabaseCommand(0, "No Errors", catalogs);
        } // end method ExecuteDisplayCommand

        /// <summary>Executes a display users command.</summary>
        /// <returns>A return command containing a list of all users in the database.</returns>
        private DatabaseCommand ExecuteDisplayUsersCommand()
        {
            List<Credentials> users = new List<Credentials>();
            MySqlDataReader reader = null;

            try
            {
                MySqlLock.WaitOne();
                string query = "SELECT * FROM " + s_MYSQL_DB_NAME + "." + s_CREDENTIALS_TABLE + ";";

                MySqlCommand cmd = new MySqlCommand(query, DB_CONNECTION);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    uint ui_WP = reader.GetUInt32(1);

                    string s_username = reader.GetString(0),
                           s_courseName = reader.GetString(2);

                    bool b_admin = reader.GetBoolean(3),
                         b_active = reader.GetBoolean(5);

                    byte[] salt = new byte[i_SALT_LENGTH];
                    reader.GetBytes(4, 0, salt, 0, i_SALT_LENGTH);

                    users.Add(new Credentials(s_username, ui_WP, b_admin, b_active, salt, ""));
                } // end while

                reader.Close();
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH display catalogs command failed. Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving list of all catalogs failed. Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return new DatabaseCommand(0, "No Errors", null, null, users);
        } // end method ExecuteDisplayCommand

        /// <summary>Executes a display courses command.</summary>
        /// <returns>A return command containing a list of all courses in the database.</returns>
        private DatabaseCommand ExecuteDisplayCoursesCommand()
        {
            List<Course> courses = new List<Course>();
            MySqlDataReader reader = null;

            try
            {
                // SELECT s_COURSES_KEY WHERE *
                MySqlLock.WaitOne();
                string query = "SELECT * FROM " + s_MYSQL_DB_NAME + "." + s_COURSES_TABLE +  ";";

                MySqlCommand cmd = new MySqlCommand(query, DB_CONNECTION);
                reader = cmd.ExecuteReader();
                

                while(reader.Read())
                {
                    uint ui_WP = reader.GetUInt32(1),
                         ui_credits = reader.GetUInt32(7),
                         ui_preRequsCount = reader.GetUInt32(9);

                    string s_courseID   = reader.GetString(0),
                           s_courseName = reader.GetString(2),
                           s_department = reader.GetString(8);

                    bool[] ba_offered = new bool[4] { reader.GetBoolean(3), reader.GetBoolean(4), reader.GetBoolean(5), reader.GetBoolean(6) };

                    List<string> ls_preRequs = new List<string>();

                    for (int i = 0; i < ui_preRequsCount; i++)
                    {
                        if (reader.FieldCount > (i + 10))
                        {
                            ls_preRequs.Add(reader.GetString(i + 10));
                        } // end if
                        else
                        {
                            WriteToLog(" -- DBH the record for course " + s_courseID + " is corrupted! Prerequ counter does not match the number of prerequs in database.");
                            break;
                        } // end else
                    } // end for

                    Course temp = new Course(s_courseName, s_courseID, ui_credits, false, ba_offered, ls_preRequs);
                    temp.Department = s_department;
                    temp.WP = ui_WP;

                    courses.Add(temp);
                } // end while

                reader.Close();                
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH display courses command failed. Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving list of all courses failed. Msg: " + e.Message);
            } // end catch
            finally
            {
                if(!reader.IsClosed)
                {
                    reader.Close();
                } // end if

                MySqlLock.ReleaseMutex();

            } // end finally 

            return new DatabaseCommand(0, "No Errors", null, courses);
        } // end method ExecuteDisplayCommand

        /// <summary>Executes a retrieve password salt command.</summary>
        /// <param name="cmd">Command containing the credentials of the user whose salt is to be retrieved.</param>
        /// <returns>A return command containing the password salt for the requested user.</returns>
        private DatabaseCommand ExecuteGetSaltCommand(DatabaseCommand cmd)
        {
            Credentials cred = (Credentials)cmd.Operand;

            try
            {
                cred = (Credentials)Retrieve(cred.UserName, 'U');
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve salt command failed for user " + cred.UserName + ". Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving the salt for " + cred.UserName + " failed. Msg: " + e.Message);
            } // end catch
            return new DatabaseCommand(CommandType.Return, cred);
        } // end method ExecuteGetSaltCommand

        #endregion

        /// <summary>The listener which waits for a command, blocking the run method.</summary>
        /// <param name="stream">Network stream to the connected client.</param>
        /// <returns>The command that is to be executed.</returns>
        private DatabaseCommand WaitForCommand(NetworkStream stream)
        {
            byte[] ba_data = new byte[BUFFER_SIZE];

            MemoryStream ms = new MemoryStream();

            DateTime arrival = DateTime.Now;

            while (!stream.DataAvailable)
            {
                DateTime current = DateTime.Now;

                if (current.Hour - arrival.Hour >= 3)
                { // Kill thread after 8 hours in wait loop
                    return new DatabaseCommand(CommandType.Disconnect);
                } // end if
            } // end while

            // read from stream in chunks of 2048 bytes
            for (int i = 0; stream.DataAvailable; i++)
            {
                int bytes = 0;
                if (stream.CanRead)
                {
                    bytes = stream.Read(ba_data, 0, BUFFER_SIZE);
                } // end if
                ms.Write(ba_data, 0, bytes);
            } // end for


            BinaryFormatter formatter = new BinaryFormatter();

            ms.Position = 0;

            DatabaseCommand cmd = (DatabaseCommand)formatter.Deserialize(ms);
            return cmd;
        } // end method WaitForCommand

        /// <summary>Sends the command back to the sender of the initial request.</summary>
        /// <param name="cmd">The return command to be sent.</param>
        /// <param name="stream">Network stream to send the data back.</param>
        /// <returns>0 or error code.</returns>
        private int SendResult(DatabaseCommand cmd, NetworkStream stream)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(ms, cmd);

                stream.Write(ms.ToArray(), 0, ms.ToArray().Length);
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH Serialisation of return command failed. Msg: " + e.Message);
                return 1;
            } // end catch

            return 0;
        } // end method SendResult

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region DBH Management
        /// <summary>Logs the DBH into the MySQL database.</summary>
        /// <returns>True if successful, false otherwise.</returns>
        private bool Login()
        {
            Console.Write("Please enter password for {0}: ", s_MYSQL_DB_NAME);

            for (int i = 10; i > 0; i--)
            {
                if (i < 10)
                {
                    WriteToLog(" -- DBH A login attempt failed, remaining tries: " + i.ToString());
                    Console.WriteLine("\n\nPassword was invalid. Tries remaining: {0}", i.ToString());
                    Console.Write("Please enter password for {0}: ", s_MYSQL_DB_NAME);
                } // end if

                string s_pw = "";
                ConsoleKeyInfo key;

                do // get password loop
                {
                    key = Console.ReadKey(true); // extract keys w/o displaying them

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (s_pw.Length > 0)
                        {
                            s_pw = s_pw.Substring(0, s_pw.Length - 1);
                            Console.Write("\b \b");
                        } // end if
                        continue;
                    } // end if

                    // ignore control keys (e.g. CTRL, ALT, etc.)
                    if (char.IsControl(key.KeyChar))
                    {
                        continue;
                    } // end if

                    // append key to password string
                    s_pw += key.KeyChar;
                    Console.Write("*"); // display a star in console
                } while (key.Key != ConsoleKey.Enter);

                Console.WriteLine("\nAttempting to log in ...");

                if (ConnectToDB(ref s_pw))
                {
                    WriteToLog(" -- DBH A user sucessfully logged in, remaining tries: " + i.ToString());
                    Console.WriteLine("Login was successful.");
                    return true;
                } // end if
            } // end for

            return false;
        } // end method Login

        /// <summary>Sets up all necessary components for DBH.</summary>
        /// <returns>An error code or 0 if setup was successful.</returns>
        /// <exception cref="Exception">Thrown if it the master record could not be retrieved from the database.</exception>
        /// <remarks>Setup will block until an initial website connection is established.</remarks>
        public int SetUp(bool login = true)
        {
            if (login)
            {
                if (!Login())
                {
                    return 1; // login attempt failed
                } // end if
            } // end if

            // RNG for salt creation
            RNG = new RNGCryptoServiceProvider();

            // TCP setup
            if (s_IP_ADDRESS == "Any")
            {
                address = IPAddress.Any;
                WriteToLog(" -- DBH Is accepting data from any client.");
            } // end if
            else
            {
                address = IPAddress.Parse(s_IP_ADDRESS);
                WriteToLog(" -- DBH Is only acception connections from IP " + s_IP_ADDRESS);
            } // end else

            tcpListener = new TcpListener(address, i_TCP_PORT);

            ui_COL_COUNT = new uint[4];

            GetColumnCounts("", 0, true);

            clientThreads = new List<Thread>();
            clients = new List<TcpClient>();

            tcpListener.Start();

            return 0;
        } // end method SetUp

        /// <summary>Cleans up after DBH is closed.</summary>
        public void CleanUp()
        {
            if (DB_CONNECTION != null)
            {
                DB_CONNECTION.Close();
            } // end if

            RNG.Dispose();
            tcpListener.Stop();

            foreach (TcpClient t in clients)
            {
                t.Close();
            } // end foreach
            foreach (Thread t in clientThreads)
            {
                t.Abort();
            } // end foreach
        } // end method CleanUp

        /// <summary>Sets the ui_COL_COUNT array to contain the number of columns in all variable length tables.</summary>
        /// <param name="s_table">Optional. The table to check, leaving this empty will check all tables.</param>
        /// <param name="i_index">Optional. The index of the table in arg 1. </param>
        private void GetColumnCounts(string s_table = "", int i_index = 0, bool all = false)
        {
            string[] table_names = { s_PLAN_TABLE, s_COURSES_TABLE, s_CATALOGS_TABLE, s_DEGREES_TABLE};
            uint[]   offset      = {      3,             10,               3,                5       };

            for (int i = 0; i < 4; i++)
            {
                string query = "SELECT count(*) FROM information_schema.columns WHERE table_schema = \"" + s_MYSQL_DB_NAME + "\"";
                query += " AND table_name = \"" + table_names[i] + "\"";

                MySqlCommand cmd = new MySqlCommand(query, DB_CONNECTION);

                ui_COL_COUNT[i] = Convert.ToUInt32(cmd.ExecuteScalar());

                ui_COL_COUNT[i] -= offset[i];
            } // end for
        } // end method GetColumnCounts

        /// <summary>Writes the given message into the DBH log with a current time stamp.</summary>
        /// <param name="s_msg">The message to log.</param>
        public static void WriteToLog(string s_msg)
        {
            // Variables:
            string s_timeStamp = DateTime.Today.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();

            LogLock.WaitOne();
            StreamWriter log = new StreamWriter(s_logFilePath, true);

            log.WriteLine(s_timeStamp + s_msg);

            log.Close();
            LogLock.ReleaseMutex();
        } // end method WriteToLog

        /// <summary>
        ///     This method attempts to lock the database lock, if it does not succeed 
        ///     at least once per two-minute period, it will kill the application.
        /// </summary>
        private void DeadLockDetector()
        {
            for (;;)
            {
                if (!MySqlLock.WaitOne(120000))
                {
                    Environment.Exit(0);
                    return;
                } // end if
                else
                {
                    MySqlLock.ReleaseMutex();
                    Thread.Sleep(120000);
                } // end else
            } // end forever loop
        } // end method DeadLockDetector

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region Website Control
        // Website Login Handler:
        /// <summary>Checks the password entered by a user against the database record.</summary>
        /// <param name="s_ID">The username of the person attempting to log in.</param>
        /// <param name="s_pw">A string containing the user's password hash.</param>
        /// <returns>True if the password entered matches the database record and out parameter b_isAdmin.</returns>
        /// <remarks>The parameter ss_pw will be destroyed during method execution, and can not be used afterwards.</remarks>
        public bool LoginAttempt(string s_ID, string s_pw)
        {
            // Variables:
            var output = false;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "*");

            string s_temp = String.Empty;

            try
            {
                MySqlLock.WaitOne();
                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows) // check if user exists
                {
                    s_temp = reader.GetString(2);

                    // check if passwords match
                    if (s_pw == s_temp)
                    {
                        WriteToLog(" -- DBH the user " + s_ID + " sucessfully logged in.");

                        output = true; // login sucessful
                    } // end if
                    else
                    {
                        WriteToLog(" -- DBH the user " + s_ID + " entered an incorrect password, login failed.");
                    } // end else                    
                } // end if
                else
                {
                    WriteToLog(" -- DBH the user " + s_ID + " does not exist, login failed.");
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            finally
            {
                // end DB connection
                reader.Close();
                MySqlLock.ReleaseMutex();
            } // end finally            

            return output;
        } // end method LoginAttempt

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region General Database
        /// <summary>Retrieves an object from the specified database.</summary>
        /// <param name="s_ID">The key associated with this object.</param>
        /// <param name="c_type">The type of object to retrieve.</param>
        /// <param name="b_shallow">Whether or not to retrieve a shallow object.</param>
        /// <returns>The requested object.</returns>
        /// <exception cref="RetrieveError">Thrown if an invalid type is passed in arg 2.</exception>
        private object Retrieve(string s_ID, char c_type, bool b_shallow = false)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        return RetrieveStudent(s_ID);
                    case 'Y':
                        return RetrieveCatalog(s_ID, b_shallow);
                    case 'C':
                        Course temp = RetrieveCourse(s_ID, b_shallow, 0);
                        return temp;
                    case 'U':
                        return RetrieveUserCredentials(s_ID);
                    case 'P':
                        return RetrieveStudentPlan(s_ID);
                    default:
                        throw new RetrieveError("Invalid character received by Retrieve method.", c_type);
                } // end switch
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                WriteToLog(" -- DBH retrieve could not find the key " + s_ID + "of type " + c_type + "Msg: " + e.Message);
                return null;
            } // end catch
        } // end method Retrieve

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL methods

        #region MySQL Retrieve

        /// <summary>Retrieves the requested student plan information from the database.</summary>
        /// <param name="s_ID">The key associated with the requested plan.</param>
        /// <returns>A PlanInfo structure containing the requested info.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        private PlanInfo RetrieveStudentPlan(string s_ID)
        {
            // Variables:
            string[] sa_data = null;

            string s_qtr = null;

            uint ui_WP = 0;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");
            
            try
            {
                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    // Variables:
                    int i_offset = 3; // offset for the SID, WP, and start quarter columns
                    int i_fields = reader.FieldCount - i_offset; // number of rows in actual plan


                    sa_data = new string[i_fields];

                    ui_WP = reader.GetUInt32(1);
                    s_qtr = reader.GetString(2);

                    // extract classes in plan
                    for (int i = 0; i < i_fields; i++)
                    {
                        string temp = reader.GetString(i + i_offset);

                        if (temp != null)
                        {
                            sa_data[i] = string.Copy(temp);
                        } // end if
                        else
                        {
                            break;
                        } // end else
                    } // end while
                } // end if
                else // if the reader has no rows, then the key doesn't exist in the DB
                {
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve student plan encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if(reader != null)
                {
                    reader.Close();
                } // end if
                
                MySqlLock.ReleaseMutex();
            } // end finally            

            return new PlanInfo(s_ID, ui_WP, s_qtr, sa_data);
        } // end method RetrieveStudentPlan

        /// <summary>Retrieves the requested user credentials from the database.</summary>
        /// <param name="s_ID">The key associated with the requested credentials.</param>
        /// <returns>A Credentials structure containing the requested info.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        private Credentials RetrieveUserCredentials(string s_ID)
        {
            // Variables:
            Credentials credentials;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "*");

            try
            {
                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    // Variables:
                    byte[] ba_salt = new byte[i_SALT_LENGTH];

                    bool b_isAdmin = reader.GetBoolean(3),
                        b_isActive = reader.GetBoolean(5);

                    uint i_WP = reader.GetUInt32(1);

                    reader.GetBytes(4, 0, ba_salt, 0, i_SALT_LENGTH);

                    credentials = new Credentials(s_ID, i_WP, b_isAdmin, b_isActive, ba_salt, "");
                } // end if
                else
                {
                    WriteToLog(" -- DBH the requested user could not be located in the database.");
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve credentials encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                } // end if

                MySqlLock.ReleaseMutex();
            } // end finally

            return credentials;
        } // end method RetrieveUserCredentials

        /// <summary>Retrieves the requested catalog from the database.</summary>
        /// <param name="s_ID">The ID associated with this catalog.</param>
        /// <param name="b_shallow">Whether or not to retrieve shallow copies of courses inside the DegreeRequirements.</param>
        /// <returns>A CatalogRequirements object containing the requested info.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        private CatalogRequirements RetrieveCatalog(string s_ID, bool b_shallow)
        {
            CatalogRequirements catalog;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_CATALOGS_TABLE, s_CATALOGS_KEY, "*");

            try
            {
                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1),
                         ui_numDegrees = reader.GetUInt32(2);

                    List<string> l_degrees = new List<string>();

                    for(int i = 0; i < ui_numDegrees; i++)
                    {
                         l_degrees.Add(reader.GetString(i + 3));
                    } // end for

                    reader.Close();
                    MySqlLock.ReleaseMutex();

                    List<DegreeRequirements> requs = new List<DegreeRequirements>();

                    foreach (string s in l_degrees)
                    {
                        try
                        {
                            requs.Add(RetrieveDegree(s, b_shallow, 0));
                        } // end try
                        catch (ThreadAbortException e)
                        {
                            throw e;
                        } // end catch
                        catch (KeyNotFoundException)
                        {
                            WriteToLog(" -- DBH The catalog " + s_ID + " contains a reference to the degree " + s + " but this degree does not exist in the database.");
                        } // end catch
                        catch (Exception e)
                        {
                            WriteToLog(" -- DBH Retrieve catalog encountered an error. Msg: " + e.Message );
                        } // end catch
                    } // end foreach

                    catalog = new CatalogRequirements(s_ID, requs);
                    catalog.WP = ui_WP;
                } // end if
                else
                {
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                MySqlLock.ReleaseMutex();
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve catalog encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                MySqlLock.ReleaseMutex();
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                } // end if
            } // end finally

            return catalog;
        } // end method RetrieveCatalog

        /// <summary>Retrieves the list of degrees associated with a given catalog.</summary>
        /// <param name="s_ID">The ID of the catalog to which the degrees belong.</param>
        /// <param name="b_shallow">Whether or not to retrieve a shallow version of the requested DegreeRequirements object.</param>
        /// <param name="ui_depth">The current depth of recursion, to prevent excessive/infinite recursion loops.</param>
        /// <returns>A list of DegreeRequirement structures associated with the given degree.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        /// <exception cref="RecursionDepthException">Thrown the depth of recursion exceeds the limit as defined in <see cref="ui_MAX_RECURSION_DEPTH"/></exception>
        private DegreeRequirements RetrieveDegree(string s_ID, bool b_shallow, uint ui_depth)
        {
            if (ui_depth == ui_MAX_RECURSION_DEPTH)
            {
                throw new RecursionDepthException("Retrieving the degree " + s_ID + " caused a recursion depth of " + ui_depth + " stopping to prevent infinite recursion.");
            } // end if

            DegreeRequirements degree;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_DEGREES_TABLE, s_DEGREES_KEY, "*");

            try
            {
                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();
                reader.Read();


                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1),
                         ui_numCourses = reader.GetUInt32(4);

                    string s_name = reader.GetString(2),
                           s_deparment = reader.GetString(3);

                    List<string> l_courses = new List<string>();

                    for(int i = 0; i < ui_numCourses; i++)
                    {
                        string crs = reader.GetString(i + 5);
                        l_courses.Add(crs);
                    } // end for

                    reader.Close();
                    MySqlLock.ReleaseMutex();                    

                    if (!b_shallow)
                    {
                        List<Course> requs = new List<Course>();

                        foreach (string s in l_courses)
                        {
                            try
                            {
                                requs.Add(RetrieveCourse(s, b_shallow, ui_depth + 1));
                            } // end try
                            catch (ThreadAbortException e)
                            {
                                throw e;
                            } // end catch
                            catch (KeyNotFoundException)
                            {
                                WriteToLog(" -- DBH The degree " + s_ID + " contains a reference to the course " + s + " but this course does not exist in the database.");
                                WriteToLog(" -- DBH The degree " + s_ID + " will be automatically updated to remove this.");
                            } // end catch
                            catch (Exception e)
                            {
                                WriteToLog(" -- DBH Retrieve degree encountered an error. Msg: " + e.Message);
                            } // end catch
                        } // end foreach

                        degree = new DegreeRequirements(s_ID, s_name, s_deparment, requs);
                    } // end if
                    else
                    {
                        degree = new DegreeRequirements(s_ID, s_name, s_deparment, l_courses);
                    } // end else
                } // end if
                else
                {
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                MySqlLock.ReleaseMutex();
                throw e;
            } // end catch
            catch (AbandonedMutexException e)
            {
                WriteToLog(" -- DBH A thread abandoned an open mutex lock, attempting to release it now.");
                WriteToLog(" -- DBH Message from exception: " + e.Message);
                e.Mutex.ReleaseMutex();
                throw new Exception("Abandoned mutex lock! Msg: " + e.Message);
            } // end catch
            catch (RecursionDepthException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve degree encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                MySqlLock.ReleaseMutex();
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                } // end if
            } // end finally

            return degree;
        } // end method RetrieveDegrees

        /// <summary>Retrieves the requested student from the database.</summary>
        /// <param name="s_ID">The SID of the student to retrieve.</param>
        /// <returns>A student object containing the requested info.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        private Student RetrieveStudent(string s_ID)
        {
            Student student;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_STUDENTS_TABLE, s_STUDENTS_KEY, "*");
            
            try
            {
                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1),
                         ui_start_year = reader.GetUInt32(5),
                         ui_expected_year = reader.GetUInt32(7);

                    string s_fName = reader.GetString(2),
                           s_lName = reader.GetString(3),
                           s_start_season = reader.GetString(4),
                           s_expected_season = reader.GetString(6);

                    Quarter q_start = new Quarter(ui_start_year, (Season)Enum.Parse(typeof(Season), s_start_season));

                    if (ui_expected_year > 0)
                    {
                        Quarter q_expected_grad = new Quarter(ui_expected_year, (Season)Enum.Parse(typeof(Season), s_expected_season));
                        student = new Student(new Name(s_fName, s_lName), s_ID, q_start)
                        {
                            ExpectedGraduation = q_expected_grad
                        }; // end student Initializer
                        student.WP = ui_WP;
                    } // end if
                    else
                    {
                        student = new Student(new Name(s_fName, s_lName), s_ID, q_start);
                        student.WP = ui_WP;
                    } // end else
                } // end if
                else
                {
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve student encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                } // end if

                MySqlLock.ReleaseMutex();
            } // end finally

            return student;
        } // end method RetrieveStudent

        /// <summary>Retrieves the requested course from the database.</summary>
        /// <param name="s_ID">The id of the course to retrieve.</param>
        /// <param name="b_shallow">Whether or not to create a shallow course object.</param>
        /// <param name="ui_depth">The current depth of recursion, to prevent excessive/infinite recursion loops.</param>
        /// <returns>A course object containing the requested info.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key passed in arg 1 does not exist in the database.</exception>
        /// <exception cref="RecursionDepthException">Thrown the depth of recursion exceeds the limit as defined in <see cref="ui_MAX_RECURSION_DEPTH"/></exception>
        /// <remarks>
        ///     A shallow course object does not contain a list of course prerequisites, but rather a list of strings
        ///     with the IDs of all course prerequisites.                     
        /// </remarks>
        private Course RetrieveCourse(string s_ID, bool b_shallow, uint ui_depth)
        {
            if (ui_depth == ui_MAX_RECURSION_DEPTH)
            {
                throw new RecursionDepthException("Retrieving the course " + s_ID + " caused a recursion depth of " + ui_depth + " stopping to prevent infinite recursion.");
            } // end if

            Course course;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_COURSES_TABLE, s_COURSES_KEY, "*");

            try
            {
                MySqlLock.WaitOne();
                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1),
                         ui_credits = reader.GetUInt32(7),
                         ui_preRequsCount = reader.GetUInt32(9);

                    string s_courseName = reader.GetString(2),
                           s_department = reader.GetString(8);

                    bool[] ba_offered = new bool[4] {reader.GetBoolean(3), reader.GetBoolean(4), reader.GetBoolean(5), reader.GetBoolean(6) };

                    List<string> ls_preRequs = new List<string>();

                    for(int i = 0; i < ui_preRequsCount; i++)
                    {
                        ls_preRequs.Add(reader.GetString(i + 10));
                    } // end for

                    reader.Close();
                    MySqlLock.ReleaseMutex();

                    if (!b_shallow)
                    {
                        List<Course> lc_prerequs = new List<Course>();

                        foreach (string s in ls_preRequs)
                        {
                            lc_prerequs.Add(RetrieveCourse(s, false, ui_depth+1));
                        } // end foreach

                        course = new Course(s_courseName, s_ID, ui_credits, false, ba_offered, lc_prerequs);
                        course.Department = s_department;
                        course.WP = ui_WP;
                    } // end if
                    else
                    {
                        course = new Course(s_courseName, s_ID, ui_credits, false, ba_offered, ls_preRequs);
                        course.Department = s_department;
                        course.WP = ui_WP;
                    } // end else
                } // end if
                else
                {
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (KeyNotFoundException e)
            {
                MySqlLock.ReleaseMutex();
                throw e;
            } // end catch
            catch (AbandonedMutexException e)
            {
                WriteToLog(" -- DBH A thread abandoned an open mutex lock, attempting to release it now.");
                WriteToLog(" -- DBH Message from exception: " + e.Message);
                e.Mutex.ReleaseMutex();
                throw new Exception("Abandoned mutex lock! Msg: " + e.Message);
            } // end catch
            catch (RecursionDepthException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                MySqlLock.ReleaseMutex();
                WriteToLog(" -- DBH retrieve course encountered an unknown error. Msg: " + e.Message);
                WriteToLog(" -- DBH ignoring error, and throwing key not found. Msg: " + e.Message);
                throw new KeyNotFoundException(s_ID);
            } // end catch
            finally
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                } // end if
            } // end finally

            return course;
        } // end method RetrieveCourse

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL Update
        /// <summary>Updates a student plan in the MySQL database.</summary>
        /// <param name="plan">The plan to update.</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <remarks>
        ///         If no plan with plan.StudentID exists, a new plan is created.
        /// </remarks>
        private int Update(PlanInfo plan)
        {
            try
            {
                plan.Classes[0] = plan.Classes[0].Replace("\"", "\\\"");

                MySqlCommand cmd = GetCommand(plan.StudentID, 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();


                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if ((ui_WP != plan.WP) && false)
                    {
                        WriteToLog(" -- DBH update failed for graduation plan of  " + plan.StudentID + " because of write protection.");
                        return 1;
                    } // end if

                    ui_WP++;

                    // update write protection
                    MySqlCommand temp = GetCommand(plan.StudentID, 'U', s_PLAN_TABLE, s_PLAN_KEY, "WP", ui_WP.ToString());
                    temp.ExecuteNonQuery();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[0] < plan.Classes.Length)
                    {
                        int k = plan.Classes.Length - (int)ui_COL_COUNT[0]; // number of columns that must be added
                        AddColumns(k, s_PLAN_TABLE, "qtr_");
                    } // end if

                    // store new data
                    temp = GetCommand(plan.StudentID, 'U', s_PLAN_TABLE, s_PLAN_KEY, "start_qtr", plan.StartQuarter.ToString());
                    temp.ExecuteNonQuery();

                    int i = 0;

                    foreach (string s in plan.Classes)
                    {
                        temp = GetCommand(plan.StudentID, 'U', s_PLAN_TABLE, s_PLAN_KEY, "qtr_" + i.ToString(), s);
                        temp.ExecuteNonQuery();
                        i++;
                    } // end foreach

                } // end if
                else
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[0] < plan.Classes.Length)
                    {
                        int k = plan.Classes.Length - (int)ui_COL_COUNT[0]; // number of columns that must be added
                        AddColumns(k, s_PLAN_TABLE, "qtr_");
                    } // end if

                    MySqlCommand command = GetCommand(plan.StudentID, 'I', s_PLAN_TABLE, s_PLAN_KEY, "", GetInsertValues(plan));
                    command.ExecuteNonQuery();
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the student plan with ID: " + plan.StudentID + " Msg: " + e.Message);
                return 2;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method Update(PlanInfo)

        /// <summary>Updates a credentials record in the MySQL database.</summary>
        /// <param name="credentials">The credentials to update.</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <remarks>
        ///         If no user with credentials.UserName exists, a new user is created.
        ///         The new user will have a blank password, and UpdatePassword must be called
        ///         to activate the user, and assign a password.
        /// </remarks>
        private int Update(Credentials credentials)
        {
            var output = 0;

            try
            {
                MySqlCommand cmd = GetCommand(credentials.UserName, 'S', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();

                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if ((ui_WP != credentials.WP) && false)
                    {
                        WriteToLog(" -- DBH update failed for credentials of " + credentials.UserName + " because of write protection.");
                        output = 1;
                    } // end if

                    ui_WP++;
                    MySqlCommand temp = GetCommand(credentials.UserName, 'U', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "WP", ui_WP.ToString());
                    temp.ExecuteNonQuery();
                    temp = GetCommand(credentials.UserName, 'U', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "admin", credentials.IsAdmin.ToString());
                    temp.ExecuteNonQuery();
                } // end if
                else
                {
                    reader.Close();
                    output = CreateUser(credentials);
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the user credentials with username: " + credentials.UserName + " Msg: " + e.Message);
                output = 2;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method Update(Credentials)

        /// <summary>Updates a course in the MySQL database.</summary>
        /// <param name="course">The course to update.</param>
        /// <returns>0 or an error code.</returns>
        private int Update(Course course)
        {
            try
            {
                MySqlCommand cmd = GetCommand(course.ID, 'S', s_COURSES_TABLE, s_COURSES_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();


                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if ((ui_WP != course.WP) && false)
                    {
                        WriteToLog(" -- DBH update failed for course  " + course.ID + " because of write protection.");
                        return 1;
                    } // end if

                    ui_WP++;                   

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[1] < course.ShallowPreRequisites.Count)
                    {
                        int k = course.ShallowPreRequisites.Count - (int)ui_COL_COUNT[1]; // number of columns that must be added
                        AddColumns(k, s_COURSES_TABLE, "pre_requ_");
                    } // end if

                    // UPDATE `test_db`.`courses` SET `course_id`='CS123', `WP`='2', `course_name`='Introas To CS', `offered_winter`='1', 
                    //'offered_spring`='1', `offered_summer`='1', `offered_fall`='1', `num_credits`='2', `department`='as', `num_pre_requs`='4' WHERE `course_id`='CS110';

                    string query = "UPDATE " + s_MYSQL_DB_NAME + "." + s_COURSES_TABLE + " SET WP = " + ui_WP.ToString() + ", course_name = \"" + course.Name + "\", ";
                    query += "offered_winter = " + course.QuartersOffered[0].ToString() + ", offered_spring = " + course.QuartersOffered[1].ToString() + ", offered_summer = ";
                    query += course.QuartersOffered[2].ToString() + ", offered_fall = " + course.QuartersOffered[3].ToString() + ", num_credits = " + course.Credits.ToString();
                    query += ", department = \"" + course.Department + "\", num_pre_requs = " + course.ShallowPreRequisites.Count;


                    int i = 0;

                    foreach (string s in course.ShallowPreRequisites)
                    {
                        query += ", pre_requ_" + i.ToString() + " = \"" + s + "\"";
                        i++;
                    } // end foreach

                    query += " WHERE " + s_COURSES_KEY + " = \"" + course.ID + "\";";

                    MySqlCommand temp = new MySqlCommand(query, DB_CONNECTION);
                    temp.ExecuteNonQuery();
                } // end if
                else
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[1] < course.ShallowPreRequisites.Count)
                    {
                        int k = course.ShallowPreRequisites.Count - (int)ui_COL_COUNT[1]; // number of columns that must be added
                        AddColumns(k, s_COURSES_TABLE, "pre_requ_");
                    } // end if

                    string query = GetSpecialInsertQuery(s_COURSES_TABLE, GetSpecialInsertString(course), GetSpecialInsertValues(course)); //GetInsertValues(course);

                    MySqlCommand command = new MySqlCommand(query, DB_CONNECTION); //GetCommand(course.ID, 'I', s_COURSES_TABLE, s_COURSES_KEY, "", query);
                    command.ExecuteNonQuery();
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the course " + course.ID + " Msg: " + e.Message);
                return 1;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method Update(Course)

        /// <summary>Updates a Catalog in the MySQL database.</summary>
        /// <param name="catalog">The catalog object to update.</param>
        /// <returns>0 or an error code.</returns>
        private int Update(CatalogRequirements catalog)
        {
            try
            {
                MySqlCommand cmd = GetCommand(catalog.ID, 'S', s_CATALOGS_TABLE, s_CATALOGS_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();


                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if ((ui_WP != catalog.WP) && false)
                    {
                        WriteToLog(" -- DBH update failed for catalog  " + catalog.ID + " because of write protection.");
                        return 1;
                    } // end if

                    ui_WP++;

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[2] < catalog.DegreeRequirements.Count)
                    {
                        int k = catalog.DegreeRequirements.Count - (int)ui_COL_COUNT[2]; // number of columns that must be added
                        AddColumns(k, s_CATALOGS_TABLE, "degree_");
                    } // end if

                    // UPDATE `test_db`.`courses` SET `course_id`='CS123', `WP`='2', `course_name`='Introas To CS', `offered_winter`='1', 
                    //'offered_spring`='1', `offered_summer`='1', `offered_fall`='1', `num_credits`='2', `department`='as', `num_pre_requs`='4' WHERE `course_id`='CS110';

                    string query = "UPDATE " + s_MYSQL_DB_NAME + "." + s_CATALOGS_TABLE + " SET WP = " + ui_WP.ToString() + ", number_degrees = " + catalog.DegreeRequirements.Count.ToString();
                    
                    int i = 0;

                    foreach (DegreeRequirements d in catalog.DegreeRequirements)
                    {
                        query += ", degree_" + i.ToString() + " = \"" + catalog.ID + "_" + d.ID + "\"";
                        i++;
                    } // end foreach

                    query += " WHERE "+ s_CATALOGS_KEY + " = \"" + catalog.ID + "\";";

                    MySqlCommand temp = new MySqlCommand(query, DB_CONNECTION);

                    temp.ExecuteNonQuery();

                    bool b_failure = false;

                    foreach (DegreeRequirements d in catalog.DegreeRequirements)
                    {
                        int exit_code = Update(catalog, d);
                        switch (exit_code)
                        {
                            case 0:
                                break;
                            default:
                                b_failure = true;
                                WriteToLog("Updating the degree " + d.ID + " failed. See prior log entry. Error code: " + exit_code);
                                break;
                        } // end switch
                    } // end foreach

                    if (b_failure)
                    {
                        WriteToLog(" -- DBH successfully updated the catalog " + catalog.ID + ", but there was an issue updating the degrees.");
                    } // end else b_failure
                } // end if
                else
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[2] < catalog.DegreeRequirements.Count)
                    {
                        int k = catalog.DegreeRequirements.Count - (int)ui_COL_COUNT[2]; // number of columns that must be added
                        AddColumns(k, s_CATALOGS_TABLE, "degree_");
                    } // end if

                    MySqlCommand command = GetCommand(catalog.ID, 'I', s_CATALOGS_TABLE, s_CATALOGS_KEY, "", GetInsertValues(catalog));
                    command.ExecuteNonQuery();

                    bool b_failure = false;

                    foreach (DegreeRequirements d in catalog.DegreeRequirements)
                    {
                        int exit_code = Update(catalog, d);
                        switch(exit_code)
                        {
                            case 0:
                                break;
                            default:
                                b_failure = true;
                                WriteToLog("Updating the degree " + d.ID + " failed. See prior log entry. Error code: " + exit_code);
                                break;
                        } // end switch
                    } // end foreach

                    if (b_failure)
                    {
                        WriteToLog(" -- DBH successfully created the catalog " + catalog.ID + " but there was an issue creating a degree.");
                    } // end if
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the catalog with ID: " + catalog.ID + " Msg: " + e.Message);
                return 2;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method Update(CatalogRequirements)

        /// <summary>
        /// Updates the specified degree requirements asscoiated with the catalog. 
        /// Note: The caller of this method MUST own the lock to the database.
        /// </summary>
        /// <param name="catalog">The catalog associated with the DegreeRequirements object in arg2</param>
        /// <param name="degree">The DegreeRequirements to update.</param>
        /// <returns>0 or an error code</returns>
        /// <remarks>This method should only be called from within <see cref="Update(CatalogRequirements)"/></remarks>
        private int Update(CatalogRequirements catalog, DegreeRequirements degree) 
        {
            try
            {
                MySqlCommand cmd = GetCommand(catalog.ID + "_" + degree.ID, 'S', s_DEGREES_TABLE, s_DEGREES_KEY, "*");

                MySqlDataReader reader = cmd.ExecuteReader();

                reader.Read();

                if (reader.HasRows)
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[3] < degree.ShallowRequirements.Count)
                    {
                        int k = degree.ShallowRequirements.Count - (int)ui_COL_COUNT[3]; // number of columns that must be added
                        AddColumns(k, s_DEGREES_TABLE, "course_");
                    } // end if

                    // UPDATE `test_db`.`courses` SET `course_id`='CS123', `WP`='2', `course_name`='Introas To CS', `offered_winter`='1', 
                    //'offered_spring`='1', `offered_summer`='1', `offered_fall`='1', `num_credits`='2', `department`='as', `num_pre_requs`='4' WHERE `course_id`='CS110';

                    string query = "UPDATE " + s_MYSQL_DB_NAME + "." + s_DEGREES_TABLE + " SET degree_name = \"" + degree.Name + "\", department = \"" + degree.Department + "\", num_requs = " + degree.ShallowRequirements.Count.ToString();

                    int i = 0;

                    foreach (string s in degree.ShallowRequirements)
                    {
                        query += ", course_" + i.ToString() + " = \"" + s + "\"";
                        i++;
                    } // end foreach

                    query += " WHERE " + s_DEGREES_KEY + " = \"" + catalog.ID + "_" + degree.ID + "\";";

                    MySqlCommand temp = new MySqlCommand(query, DB_CONNECTION);
                    temp.ExecuteNonQuery();
                } // end if
                else
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT[3] < degree.ShallowRequirements.Count)
                    {
                        int k = degree.ShallowRequirements.Count - (int)ui_COL_COUNT[3]; // number of columns that must be added
                        AddColumns(k, s_DEGREES_TABLE, "course_");
                    } // end if

                    string insert = GetSpecialInsertString(degree);
                    string vals = GetSpecialInsertValues(degree, catalog.ID + "_" + degree.ID);

                    MySqlCommand command = new MySqlCommand(GetSpecialInsertQuery(s_DEGREES_TABLE,insert,vals), DB_CONNECTION);

                    // This function does not work without this code. Don't ask why, just accept it.
                    StreamWriter writer = new StreamWriter("/var/aspnetcore/logs/insert.txt");
                    writer.Write(command.CommandText);
                    writer.Flush();
                    writer.Close();
                    StreamReader read = new StreamReader("/var/aspnetcore/logs/insert.txt");
                    string shit = read.ReadLine();
                    read.Close();

                    MySqlCommand ifThisFuckingWorks = new MySqlCommand(shit, DB_CONNECTION);
                    ifThisFuckingWorks.ExecuteNonQuery();
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the student plan with ID: " + catalog.ID + " Msg: " + e.Message);
                return 2;
            } // end catch

            return 0;
        } // end method Update(DegreeRequirements)

        /// <summary>Updates a student in the students MySQL database.</summary>
        /// <param name="student">The student to update or create.</param>
        /// <returns>0 or error code.</returns>
        private int Update(Student student)
        {
            MySqlDataReader reader = null;

            try
            {
                MySqlCommand cmd = GetCommand(student.ID, 'S', s_STUDENTS_TABLE, s_STUDENTS_KEY, "*");

                MySqlLock.WaitOne();

                reader = cmd.ExecuteReader();


                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if ((ui_WP != student.WP) && false)
                    {
                        WriteToLog(" -- DBH update failed for student " + student.ID + " because of write protection.");
                        return 1;
                    } // end if

                    ui_WP++;

                    // update write protection
                    MySqlCommand temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "WP", ui_WP.ToString());
                    temp.ExecuteNonQuery();

                    // store new data
                    temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "first_name", student.Name.FirstName);
                    temp.ExecuteNonQuery();

                    temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "last_name", student.Name.LastName);
                    temp.ExecuteNonQuery();

                    temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "starting_season", student.StartingQuarter.QuarterSeason.ToString());
                    temp.ExecuteNonQuery();

                    temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "starting_year", student.StartingQuarter.Year.ToString());
                    temp.ExecuteNonQuery();

                    if (student.HasExpectedGraduation)
                    { 
                        temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "expected_grad_season", student.ExpectedGraduation.QuarterSeason.ToString());
                        temp.ExecuteNonQuery();

                        temp = GetCommand(student.ID, 'U', s_STUDENTS_TABLE, s_STUDENTS_KEY, "expected_grad_year", student.ExpectedGraduation.Year.ToString());
                        temp.ExecuteNonQuery();
                    } // end if
                } // end if
                else
                {
                    reader.Close();

                    MySqlCommand command = GetCommand(student.ID, 'I', s_STUDENTS_TABLE, s_STUDENTS_KEY, "", GetInsertValues(student));
                    command.ExecuteNonQuery();
                } // end else
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the student: " + student.ID + " Msg: " + e.Message);
            } // end catch
            finally
            {
                if(!reader.IsClosed)
                {
                    reader.Close();
                } // end if

                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method Update(Student)

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL User Management

        /// <summary>Creates a new user with the specified properties.</summary>
        /// <param name="credentials">Should contain the username, and isAdmin fields. Others will be ignored.</param>
        /// <returns>True if creation was successful, false otherwise.</returns>
        /// <remarks>The database is already locked upon entering this method, thus, it does not lock the database itself.</remarks>
        private int CreateUser(Credentials credentials)
        {
            var output = 1;

            for (int i = 0; i < 10; i++) // try 10 times to ensure the salt is not the problem
            {
                byte[] temp = GetPasswordSalt();
                Array.Copy(temp, credentials.PWSalt, i_SALT_LENGTH);

                MySqlCommand cmd = GetCommand(credentials.UserName, 'I', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "", GetInsertValues(credentials));

                try
                {
                    cmd.ExecuteNonQuery(); // create record
                    output = 0;
                    break;
                } // end try
                catch (ThreadAbortException e)
                {
                    throw e;
                } // end catch
                catch (Exception e)
                {
                    WriteToLog(" -- DBH creation of user \"" + credentials.UserName + "\" failed. Msg: " + e.Message);
                    continue;
                } // end try
            } // end for

            return output;
        } // end method CreateUser

        /// <summary>Changes the password of the specified user.</summary>
        /// <param name="s_ID">The username of the person whose password is to be changed.</param>
        /// <param name="s_pw">The new password hash for this user.</param>
        /// <param name="b_activeStatus">Whether or not this user should be activated.</param>
        /// <returns>True if the password change was successful, otherwise false.</returns>
        /// <remarks>The password in the secure string object should already be hashed when it is received here. Otherwise a plain text password is stored.</remarks>
        private int ChangePassword(string s_ID, string s_pw, bool b_activeStatus)
        {
            var output = 1;

            MySqlCommand cmd = GetCommand(s_ID, 'U', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "password", "\"" + s_pw + "\"");

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
                ChangeUserStatus(s_ID, b_activeStatus);
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH password change for user " + s_ID + " failed. Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method ChangePassword

        /// <summary>Activates/Deactivates a user account.</summary>
        /// <param name="s_ID">The username of the account to activate/deactivate.</param>
        /// <param name="b_newStatus">The new status. True = active, False = inactive.</param>
        /// <returns>0 or an error code.</returns>
        private int ChangeUserStatus(string s_ID, bool b_newStatus)
        {
            // check if user actually exists
            try
            {
                RetrieveUserCredentials(s_ID);
            } // end try
            catch (KeyNotFoundException e)
            {
                WriteToLog(" -- DBH The user " + s_ID + " does not exist, status could not be changed. Msg: " + e.Message);
                return 1;
            } // end catch

            MySqlCommand cmd = GetCommand(s_ID, 'U', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "active", b_newStatus ? "1" : "0");

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH The user " + s_ID + "'s status could not be changed. Msg: " + e.Message);
                return 1;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method ChangeUserStatus

        /// <summary>Creates a random 256 bit salt for login credentials.</summary>
        /// <returns>A byte array filled with a random sequence of bytes.</returns>
        private byte[] GetPasswordSalt()
        {
            byte[] ba_salt = new byte[i_SALT_LENGTH];

            RNG.GetNonZeroBytes(ba_salt);

            return ba_salt;
        } // end method GetPasswordSalt

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL Delete

        /// <summary>Deletes a record from the credentials database.</summary>
        /// <param name="cred">The user to delete.</param>
        /// <returns>0 or error code.</returns>
        private int DeleteRecord(Credentials cred)
        {
            // Variables:
            MySqlCommand cmd = GetCommand(cred.UserName, 'D', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY);

            var output = 1;

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
            } // end try
            catch(ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the user " + cred.UserName + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord(Credentials)

        /// <summary>Deletes a record from the graduation plan database.</summary>
        /// <param name="plan">The plan to delete.</param>
        /// <returns>0 or error code.</returns>
        private int DeleteRecord(PlanInfo plan)
        {
            // Variables:
            MySqlCommand cmd = GetCommand(plan.StudentID, 'D', s_PLAN_TABLE, s_PLAN_KEY);

            var output = 1;

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the plan belonging to " + plan.StudentID + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord(PlanInfo)

        /// <summary>Deletes a record from the student database.</summary>
        /// <param name="student">The student to delete.</param>
        /// <returns>0 or error code.</returns>
        private int DeleteRecord(Student student)
        {
            MySqlCommand cmd = GetCommand(student.ID, 'D', s_STUDENTS_TABLE, s_STUDENTS_KEY);

            var output = 1;

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the student " + student.ID + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord(Student)

        /// <summary>Deletes a record from the course database.</summary>
        /// <param name="course">The plan to course.</param>
        /// <returns>0 or error code.</returns>
        private int DeleteRecord(Course course)
        {
            MySqlCommand cmd = GetCommand(course.ID, 'D', s_COURSES_TABLE, s_COURSES_KEY);
            var output = 1;

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the course " + course.ID + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord(Course)

        /// <summary>Deletes a record from the catalog database.</summary>
        /// <param name="catalog">The catalog to delete.</param>
        /// <returns>0 or error code.</returns>
        private int DeleteRecord(CatalogRequirements catalog)
        {
            MySqlCommand cmd = GetCommand(catalog.ID, 'D', s_CATALOGS_TABLE, s_CATALOGS_KEY);

            var output = 1;

            try
            {
                MySqlLock.WaitOne();
                cmd.ExecuteNonQuery();
                output = 0;
            } // end try
            catch (ThreadAbortException e)
            {
                throw e;
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the catalog " + catalog.ID + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord(Catalog)

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL Query Generators

        /// <summary>Creates an insert query based on the input.</summary>
        /// <param name="s_table">Table to insert to.</param>
        /// <param name="s_values">Values for the new row.</param>
        /// <returns>A ready to go insert sql query.</returns>
        private string GetInsertQuery(string s_table, string s_values)
        {
            //"INSERT INTO test_db.student_plans\nVALUES(\"22345678\", 1, \"Winter14\", \"CS101,CS110,GE1,UNIV101\")"
            // INSERT INTO test_db.course (course_id, course_name, offered_winter, offered_fall, offered_spring, offered_fall, num_credits, deparment)
            // INSERT INTO test_db.course (course_id, course_name, offered_winter, offered_fall, offered_spring, offered_fall, num_credits, deparment, num_pre_requs, ...)
            // VALUES ("", "", );

            string query = "INSERT INTO ";
            query += s_MYSQL_DB_NAME;
            query += ".";
            query += s_table;
            query += "\n VALUES(";
            query += s_values;
            query += ");";

            return query;
        } // end method GetInsertQuery


        private string GetSpecialInsertQuery(string s_table, string s_insert, string s_values)
        {
            string query = "INSERT INTO ";
            query += s_MYSQL_DB_NAME;
            query += ".";
            query += s_table;
            query += " (";
            query += s_insert;
            query += ") ";
            query += "VALUES (";
            query += s_values;
            query += ");";

            return query;
        } // end method GetSpecialInsertQuery


        private string GetSpecialInsertValues(Course course)
        {
            string values = "\"" + course.ID + "\", \"" + course.Name + "\",";

            for (int i = 0; i < course.QuartersOffered.Length; i++)
            {
                if(course.QuartersOffered[i])
                {
                    values += "1, ";
                } // end if
                else
                {
                    values += "0, ";
                } // end elses
            } // end for

            values += course.Credits.ToString() + ", ";
            values += "\"" + course.Department + "\", ";
            values += course.ShallowPreRequisites.Count;

            for (int i = 0; i < course.ShallowPreRequisites.Count; i++)
            {
                values += ",\"" + course.ShallowPreRequisites[i] + "\"";
            } // end for

            return values;
        } // end method GetSpecialInsertValues

        /// <summary>Gets a special insert string for a course object.</summary>
        /// <param name="course">The course to be added to the db.</param>
        /// <returns>The insert query.</returns>
        private string GetSpecialInsertString(Course course)
        {
            string text = s_COURSES_KEY;
            text += ", course_name, offered_winter, offered_spring, offered_summer, offered_fall, num_credits, department, num_pre_requs";

            if(course.ShallowPreRequisites.Count > 0)
            {
                for (int i = 0; i < course.ShallowPreRequisites.Count; i++)
                {
                    text += ", pre_requ_" + i;
                } // end for
            } // end if

            return text;
        } // end method GetSpecialInsertString


        private string GetSpecialInsertValues(DegreeRequirements degree, string s_ID)
        {
            string values = "\"" + s_ID + "\", 1, \"" + degree.Name + "\", \"" + degree.Department + "\", " + degree.ShallowRequirements.Count.ToString();
            
            for (int i = 0; i < degree.ShallowRequirements.Count; i++)
            {
                values += ", \"" + degree.ShallowRequirements[i] + "\" ";
            } // end for
            
            return values;
        }


        private string GetSpecialInsertString(DegreeRequirements degree)
        {
            string text = s_DEGREES_KEY;
            text += ", WP, degree_name, department, num_requs";

            if (degree.ShallowRequirements.Count > 0)
            {
                for (int i = 0; i < degree.ShallowRequirements.Count; i++)
                {
                    text += ", course_" + i;
                } // end for
            } // end if

            return text;
        } // end method GetSpecialInsertString

        /// <summary>Creates the values string for a PlanInfo object.</summary>
        /// <param name="plan">The plan to be inserted into the DB.</param>
        /// <returns>A formated string containing the values to be inserted.</returns>
        private string GetInsertValues(PlanInfo plan)
        {
            // \"{SID}\", {WP}, \"{start quarter}\", \"{quarter1 classes}\", \"{quarter2 classes}\", ...
            string s_values = "\"" + plan.StudentID + "\", 1, \"" + plan.StartQuarter + "\"";

            foreach (string s in plan.Classes)
            {
                s_values += ", \"" + s + "\"";
            } // end foreach

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates the values string for a Credentials object.</summary>
        /// <param name="credentials">The credentials to insert.</param>
        /// <returns>A formatted string ready to be used with an insert query.</returns>
        private string GetInsertValues(Credentials credentials)
        {
            string s_values = "\"" + credentials.UserName + "\", 1, \"\", " + (credentials.IsAdmin ? "1, " : "0, ");
            //                                                     pw blank
            s_values += "0x" + BitConverter.ToString(credentials.PWSalt).Replace("-", string.Empty);

            s_values += credentials.IsActive ? ", 1" : ", 0";

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates the values string for a Student object.</summary>
        /// <param name="student">The object to extract values from.</param>
        /// <returns>A string which can be used for an insert query as VALUES.</returns>
        private string GetInsertValues(Student student)
        {
            //'SID', WP, 'fName', 'lName', 'starting_season', starting_year, 'grad_season', grad_year;
            string s_values = "\"" + student.ID + "\", 1, \"" + student.Name.FirstName + "\", \"" + student.Name.LastName + "\",";

            s_values += "\"" + student.StartingQuarter.QuarterSeason.ToString() + "\", " + student.StartingQuarter.Year.ToString() + ",";
            if (student.HasExpectedGraduation)
            {
                s_values += "\"" + student.ExpectedGraduation.QuarterSeason.ToString() + "\", " + student.ExpectedGraduation.Year.ToString();
            } // end if
            else
            {
                s_values += "\"\", 0";
            } // end else

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates the values string for a Course object.</summary>
        /// <param name="course">The new course to be added to the database.</param>
        /// <returns>A string that can be used as the VALUES component of an insert query.</returns>
        private string GetInsertValues(Course course)
        {
            // 'CS110', 1, 'Intro To CS', '1', '1', '0', '1', '4', 'CS', '0');

            string s_values = "\"" + course.ID + "\", 1, \"" + course.Name + "\", ";

            foreach(bool b in course.QuartersOffered)
            {
                if(b)
                {
                    s_values += "1, ";
                } // end if
                else
                {
                    s_values += "0, ";
                } // end else
            } // end foreach

            s_values += course.Credits.ToString() + ", " ;

            s_values += "\"" + course.Department + "\",";

            if(course.ShallowPreRequisites.Count > 0)
            {
                s_values += course.ShallowPreRequisites.Count.ToString();
                foreach (string s in course.ShallowPreRequisites)
                {
                    s_values += ", \"" + s + "\"";
                } // end foreach
            } // end if
            else
            {
                s_values += "0";
            } // end if

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates the values string for a Catalog object.</summary>
        /// <param name="catalog">The new catalog to be added to the database.</param>
        /// <returns>A string that can be used as the VALUES component of an insert query.</returns>
        private string GetInsertValues(CatalogRequirements catalog)
        {
            string s_values = "\"" + catalog.ID + "\", 1, " + catalog.DegreeRequirements.Count;

            foreach (DegreeRequirements d in catalog.DegreeRequirements)
            {
                s_values += ", \"" + catalog.ID + "_" + d.ID + "\"";
            } // end foreach

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates the values string for a Degree object.</summary>
        /// <param name="catalog">The catalog associated with this degree.</param>
        /// <param name="degree">The new degree to be added to the database.</param>
        /// <returns>A string that can be used as the VALUES component of an insert query.</returns>
        private string GetInsertValues(CatalogRequirements catalog, DegreeRequirements degree)
        {
            string s_values = "\"" + catalog.ID + "_" + degree.ID + "\", 1, \"" + degree.Name + "\", \"" + degree.Department + "\", " + degree.ShallowRequirements.Count.ToString();

            foreach(string s in degree.ShallowRequirements)
            {
                s_values += ", \"" + s + "\"";
            } // emd foreach

            return s_values;
        } // end method GetInsertValues

        /// <summary>Creates an select query based on the input.</summary>
        /// <param name="s_table">The table to access.</param>
        /// <param name="s_column">The column to retrieve.</param>
        /// <param name="s_keyType">The type of key to use to locate the row.</param>
        /// <param name="s_keyValue">The value of the key.</param>
        /// <returns>A ready to go select sql query.</returns>
        private string GetSelectQuery(string s_table, string s_column, string s_keyType, string s_keyValue)
        {
            //SELECT * FROM test_db.user_credentials WHERE SID = s_ID

            string query = "SELECT ";
            query += s_column;
            query += " FROM ";
            query += s_MYSQL_DB_NAME;
            query += ".";
            query += s_table;
            query += " WHERE ";
            query += s_keyType;
            query += " = \"";
            query += s_keyValue;
            query += "\"";

            return query;
        } // end method GetSelectQuery

        /// <summary>Creates an update query based on the input.</summary>
        /// <param name="s_table">The table to access.</param>
        /// <param name="s_columnToUpdate">The column(s) to update.</param>
        /// <param name="s_keyType">The type of key to use to locate the row.</param>
        /// <param name="s_keyValue">The value of the key.</param>
        /// <param name="s_newValue">The new value for the specified column(s).</param>
        /// <returns>A ready to go update sql query.</returns>
        private string GetUpdateQuery(string s_table, string s_columnToUpdate, string s_keyType, string s_keyValue, string s_newValue)
        {
            string query = "UPDATE ";
            query += s_MYSQL_DB_NAME;
            query += ".";
            query += s_table;
            query += " SET ";
            query += s_columnToUpdate;
            query += " = ";
            if (s_columnToUpdate != "WP" && s_columnToUpdate != "admin" && s_columnToUpdate != "password" && s_columnToUpdate != "starting_year" && s_columnToUpdate != "expected_grad_year")
            {
                query += "\"";
                query += s_newValue;
                query += "\"";
            } // end if
            else
            {
                query += s_newValue;
            } // end else
            query += " WHERE ";
            query += s_keyType;
            query += "= \"";
            query += s_keyValue;
            query += "\";";
            return query;
        } // end method GetUpdateQuery

        /// <summary>Creates a delete query based on the input.</summary>
        /// <param name="s_table">The table to access.</param>
        /// <param name="s_keyType">The type of key to use to locate the row.</param>
        /// <param name="s_keyValue">The key identifying the row to delete.</param>
        /// <returns>A ready to go delete sql query.</returns>
        private string GetDeleteQuery(string s_table, string s_keyType, string s_keyValue)
        {
            string query = "DELETE FROM " + s_MYSQL_DB_NAME + "." + s_table;
            query += " WHERE " + s_keyType + " = \"" + s_keyValue + "\";";

            return query;
        } // end method GetDeleteQuery

        /// <summary>Creates a MySQL command of the specified type.</summary>
        /// <param name="s_ID">The key of the row this command shall apply to.</param>
        /// <param name="c_type">The type of command to be created.</param>
        /// <param name="s_table">The table this command should apply to.</param>
        /// <param name="s_IDType">The type of the key value passed in arg 1.</param>
        /// <param name="s_column">The column to be affected by an select/update instruction.</param>
        /// <param name="s_values">The new value for the specified column(s) for insert/update instruction.</param>
        /// <returns>A ready to be executed MySQL command of the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid command type was passed in arg 2.</exception>
        /// <remarks>
        ///          The ID should be the key for the table, either the username for credentials, or the SID for student plans.
        ///          The types of commands are: S - select, U - update, I - insert, D - delete
        ///          The ID type is either SID for student_plans table, or username for user_credentials table.
        ///          The column depends on the type of command (optional parameter):
        ///                     Select: either the column to select, or * for the whole row
        ///                     Update: the column to update, * should not be used
        ///                     Insert: unused
        ///                     Delete: unused
        ///          The values depends on the type of command (optional parameter):
        ///                     Select: unused - may be left blank
        ///                     Update: the new value of the specified column
        ///                     Insert: the new values of the specified row, must be formatted correctly
        ///                             Format: \"{SID}\", {WP}, \"{start quarter}\", \"{quarter1 classes}\", \"{quarter2 classes}\", ...
        ///                             Note: The quarters passed may not exceed the number of quarters in the table, add columns as needed before inserting
        ///                                   The classes should be in a comma separated list
        ///                     Delete: unused
        ///          Explanation of the commands: 
        ///                     Select: Will select the row with the specified ID
        ///                     Update: Will update the specified item with the new value in the row with specified ID
        ///                     Insert: Will insert a new row with specified values
        ///                     Delete: Will delete the row with specified key
        ///                     
        ///          The Master record for student plans has the ID -1, and can be updated with: GetCommand("-1", 'U', "", "")
        /// </remarks>
        private MySqlCommand GetCommand(string s_ID, char c_type, string s_table, string s_IDType, string s_column = "", string s_values = "")
        {
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = DB_CONNECTION
            };

            switch (c_type)
            {
                case 'S': // get a select command
                    cmd.CommandText = GetSelectQuery(s_table, s_column, s_IDType, s_ID);
                    break; // end case S

                case 'U': // get an update command
                    if (s_ID == "-1") // update master record command
                    {
                        cmd.CommandText = GetUpdateQuery(s_PLAN_TABLE, "WP", "SID", "-1", ui_COL_COUNT.ToString());
                    } // end if
                    else
                    {
                        cmd.CommandText = GetUpdateQuery(s_table, s_column, s_IDType, s_ID, s_values);
                    } // end else
                    break; // end case U

                case 'I': // get an insert command
                    cmd.CommandText = GetInsertQuery(s_table, s_values);
                    break; // end case I

                case 'D':
                    cmd.CommandText = GetDeleteQuery(s_table, s_IDType, s_ID);
                    break;

                default:  // invalid input
                    throw new ArgumentException("GetCommand received an invalid command type. Type received: " + c_type);
            } // end switch

            return cmd;
        } // end method GetCommand

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region MySQL Database control

        /// <summary>Connects to the MySQL database and initializes the class field DB_CONNECTION.</summary>
        /// <param name="s_pw">The password for the MySQL database.</param>
        /// <returns>True if login was successful, otherwise false.</returns>
        private bool ConnectToDB(ref string s_pw)
        {
            // Variables:
            string s_connStr = "server=" + s_MYSQL_DB_SERVER + ";port=" + s_MYSQL_DB_PORT + ";database=" +
                               s_MYSQL_DB_NAME + ";user id=" + s_MYSQL_DB_USER_ID + ";password=" + s_pw +
                               ";persistsecurityinfo=True;";
            MySqlConnection connection = new MySqlConnection(s_connStr);


            try
            {
                connection.Open();
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH connection failed. Msg: " + e.Message);
                return false;
            } // end catch

            DB_CONNECTION = connection;

            return true;
        } // end method ConnectToDB

        /// <summary>Attempts to reconnect to the MySQL database after connection was lost.</summary>
        /// <exception cref="TimeoutException">Thrown when the connection cannot be reestablished.</exception>
        private void AttemptReconnect()
        {
            try
            {
                if (DB_CONNECTION != null)
                {
                    DB_CONNECTION.Close();
                    DB_CONNECTION.Open();
                } // end if
                else // impossible to reconnect if password is lost
                {
                    WriteToLog(" -- DBH connection to MySQL DB was null when AttemptReconnect was called.");
                    throw new ArgumentNullException("The MySQL connection to " + s_MYSQL_DB_NAME + " was null while attempting to reconnect.");
                } // end else
            } // end try
            catch (MySqlException e)
            {
                WriteToLog(" -- DBH connection could not be reestablished. Msg: " + e.Message);
                throw new TimeoutException("2");
            } // end catch
            catch (Exception e)
            {
                WriteToLog(" -- DBH Attempt Reconnect encountered an exception. Msg: " + e.Message);
            } // end catch
        } // end method AttemptReconnect

        /// <summary>Keeps the DB_CONNECTION alive by accessing the DB every 2 minutes</summary>
        private void KeepAlive()
        {
            for (; ; )
            {
                try
                {
                    MySqlCommand cmd = GetCommand("-1", 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

                    try
                    {
                        MySqlLock.WaitOne();
                        MySqlDataReader reader = cmd.ExecuteReader();
                        reader.Close();
                    } // end try
                    catch(ThreadAbortException e)
                    {
                        MySqlLock.ReleaseMutex();
                        throw e;
                    } // end catch

                    MySqlLock.ReleaseMutex();
                } // end try
                catch(MySqlException e)
                {
                    WriteToLog(" -- DBH Keep alive encountered an exception when trying to test database connection. Msg: " + e.Message);
                    return;
                } // end catch
                catch (ThreadAbortException)
                {
                    WriteToLog(" -- DBH Keep alive thread received abort signal.");
                    return;
                } // end catch

                Console.WriteLine("Still Alive, sleeping 120000 ms).");

                Thread.Sleep(120000);
            } // end for
        } // end method KeepAlive
        
        /// <summary>Adds k columns to the student_plans table and updates the master record accordingly.</summary>
        /// <param name="k">Number of columns to add.</param>
        /// <param name="s_table">The table to add these new columns to.</param>
        /// <param name="name">The name of the new columns (a number will be automatically appended to this).</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool AddColumns(int k, string s_table, string name)
        {
            
            // Variables:
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = DB_CONNECTION
            };

            var output = true;

            int i = 0; // for error output in catch
            int index = 0;

            // { s_PLAN_TABLE, s_COURSES_TABLE, s_CATALOGS_TABLE, s_DEGREES_TABLE}
            if (s_table.Equals(s_PLAN_TABLE))
            {
                index = 0;
            } // end if
            else if(s_table.Equals(s_COURSES_TABLE))
            {
                index = 1;
            } // end elif
            else if(s_table.Equals(s_CATALOGS_TABLE))
            {
                index = 2;
            } // end elif
            else if(s_table.Equals(s_DEGREES_TABLE))
            {
                index = 3;
            } // end elif
            else
            {
                throw new ArgumentException("Invalid table name received: " + s_table + "");
            } // end else

            try
            {
                for (i = 0; i < k; i++)
                {
                    cmd.CommandText = "ALTER TABLE " + s_MYSQL_DB_NAME + "." + s_table + "\nADD COLUMN " + name + ui_COL_COUNT[index].ToString() + " VARCHAR(45) NULL DEFAULT '\"\"';";
                    cmd.ExecuteNonQuery();
                    ui_COL_COUNT[index]++;
                } // end for
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH add columns failed. The attempt to add " + k.ToString() + " columns to the " + s_table + " table failed. Msg: " + e.Message);
                WriteToLog(" -- DBH add columns failed. " + (i - 1).ToString() + " columns were successfully added before failing.");
                output = false;
            } // end catch

            

            return output;
        } // end method AddColumns

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #endregion
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing">unused.</param>
        void Dispose(bool disposing)
        {
            WriteToLog(" -- DBH is beginning cleanup.");
            if (!disposedValue)
            {
                if (disposing)
                {
                    CleanUp();
                } // end if

                tcpListener = null;
                DB_CONNECTION = null;

                disposedValue = true;

                WriteToLog(" -- DBH is finished cleaning up.");
            } // end if
        } // end Dispose

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        } // end Dispose
        #endregion

    } // end Class DatabaseHandler
} // end Namespace Database_Handler