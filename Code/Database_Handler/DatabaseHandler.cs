using System;
using MySql.Data.MySqlClient;
using Db4objects.Db4o;
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
    public sealed class DatabaseHandler
    {
        // Static class fields:
        /// <summary>The number of iterations for hashing the password.</summary>
        public static int i_HASH_ITERATIONS = 10000;

        /// <summary>The path to the log file which will contain the log entries created by DBH.</summary>
        public static string s_logFilePath = "log.txt";

        /// <summary>Mutex locks for databases.</summary>
        private static Mutex MySqlLock = new Mutex();
        private static Mutex StudentLock = new Mutex();
        private static Mutex CatalogLock = new Mutex();
        private static Mutex CourseLock = new Mutex();
        private static Mutex LogLock = new Mutex();


        // Class fields:
        /// <summary>Names of all database related components.</summary>
        /// <remarks>These can be set using the constructor, or the defaults can be used.</remarks>
        private readonly string s_MYSQL_DB_NAME = "test_db";
        private readonly string s_MYSQL_DB_SERVER = "localhost";
        private readonly string s_MYSQL_DB_PORT = "3306";
        private readonly string s_MYSQL_DB_USER_ID = "testuser";
        private readonly string s_CREDENTIALS_TABLE = "user_credentials";
        private readonly string s_PLAN_TABLE = "student_plans";
        private readonly string s_STUDENT_DB = "Students.db4o";
        private readonly string s_COURSE_DB = "Courses.db4o";
        private readonly string s_CATALOG_DB = "Catalogs.db4o";
        private readonly string s_CREDENTIALS_KEY = "username";
        private readonly string s_PLAN_KEY = "SID";
        private readonly string s_IP_ADDRESS = "127.0.0.1";

        private readonly int i_TCP_PORT = 44765;

        /// <summary>The length, in bytes, of the password salt.</summary>
        private const int i_SALT_LENGTH = 32;
        private const int BUFFER_SIZE = 2048;

        /// <summary>The length of the longest plan in the s_PLAN_TABLE mysql table, stored in the master record.</summary>
        private uint ui_COL_COUNT;

        /// <summary>The master connection to the MySql database, which should never be closed, except during cleanup.</summary>
        private MySqlConnection DB_CONNECTION;

        /// <summary>A RNG for building password salts.</summary>
        private RNGCryptoServiceProvider RNG;

        /// <summary>The main Tcp Listener used to talk with clients.</summary>
        private TcpListener tcpListener;

        /// <summary>Localhost IP address for the Tcp Socket</summary>
        private IPAddress address;

        /// <summary>List of all client threads currently running.</summary>
        private List<Thread> clientThreads;

        /// <summary>List of all clients currently connected to DBH.</summary>
        private List<TcpClient> clients;


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Loads the DBH configurations from the given .ini file.</summary>
        /// <param name="s_fileName">Path of the .ini file containing the configurations DBH should use.</param>
        public DatabaseHandler(string s_fileName)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(s_fileName);

            s_MYSQL_DB_NAME = data["MySql Connection"]["DB"];
            s_MYSQL_DB_SERVER = data["MySql Connection"]["host"];
            s_MYSQL_DB_PORT = data["MySql Connection"]["port"];
            s_MYSQL_DB_USER_ID = data["MySql Connection"]["user"];
            string pw = data["MySql Connection"]["password"];

            s_PLAN_TABLE = data["MySql Tables"]["grad_plans"];
            s_PLAN_KEY = data["MySql Tables"]["grad_plans_key"];
            s_CREDENTIALS_TABLE = data["MySql Tables"]["credentials"];
            s_CREDENTIALS_KEY = data["MySql Tables"]["credentials_key"];

            s_STUDENT_DB = data["DB4O Files"]["students"];
            s_COURSE_DB = data["DB4O Files"]["courses"];
            s_CATALOG_DB = data["DB4O Files"]["catalogs"];

            s_IP_ADDRESS = data["Misc"]["IP"];
            s_logFilePath = data["Misc"]["logfile_path"];
            string TCPPort = data["Misc"]["TCPIP_port"];
            i_TCP_PORT = int.Parse(TCPPort);

            ConnectToDB(ref pw);
            SetUp(false);
        } // end Constructor


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Main:
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

                    DBH.CleanUp();

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
                    StudentLock.Dispose();
                    CatalogLock.Dispose();
                    CourseLock.Dispose();
                    LogLock.Dispose();
                } // end finally
            } // end finally                    
        } // end Main


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Run:
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

            for (; ; )
            {
                TcpClient newClient = tcpListener.AcceptTcpClient();
                Thread newClientThread = new Thread(RunClient);

                // if the session expires, attempt to reopen it
                if (DB_CONNECTION.State != System.Data.ConnectionState.Open)
                {
                    WriteToLog(" -- DBH is attempting to reconnect after losing DB connection.");
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

                        break;
                    } // end catch
                } // end if

                clients.Add(newClient);
                clientThreads.Add(newClientThread);
                newClientThread.Start(newClient);

                // clean up list in case any connections are gone
                clients.RemoveAll(null);
                clientThreads.RemoveAll(delegate (Thread t) { return t.ThreadState == ThreadState.Stopped || !t.IsAlive; });
            } // end for

            return output;
        } // end method RunHost


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Command methods:
        /// <summary>Unwraps the command type, and executes the specified command.</summary>
        /// <param name="cmd">The command to be executed.</param>
        /// <param name="stream">The stream to send data back to the client.</param>
        /// <returns>An error code, or 0 if execution was successful.</returns>
        private int ExecuteCommand(DatabaseCommand cmd, NetworkStream stream)
        {
            DatabaseCommand output;

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
                case CommandType.DisplayStudents:
                    output = ExecuteDisplayStudentsCommand();
                    break;
                case CommandType.GetSalt:
                    output = ExecuteGetSaltCommand(cmd);
                    break;
                case CommandType.DisplayCourses:
                    output = ExecuteDisplayCoursesCommand();
                    break;
                case CommandType.Disconnect:
                    output = new DatabaseCommand(99, "Exit command received.");
                    break;
                default:
                    output = new DatabaseCommand(-1, "Invalid command type");
                    break;
            } // end switch

            return SendResult(output, stream);
        } // end method ExecuteCommand

        /// <summary>Executes a Retrieve command.</summary>
        /// <param name="cmd">Command containing object to be retrieved.</param>
        /// <returns>A return command containing information about execution success, and, if successful, the requested data.</returns>
        private DatabaseCommand ExecuteRetrieveCommand(DatabaseCommand cmd)
        {
            Database_Object dbo;
            Credentials cred;
            PlanInfo plan;
            DatabaseCommand result;

            switch (cmd.OperandType)
            {
                case OperandType.CatalogRequirements:
                    dbo = (CatalogRequirements)cmd.Operand;
                    result = new DatabaseCommand(CommandType.Return, (CatalogRequirements)Retrieve(dbo.ID, 'Y'), OperandType.CatalogRequirements);
                    break;
                case OperandType.Student:
                    dbo = (Student)cmd.Operand;
                    result = new DatabaseCommand(CommandType.Return, (Student)Retrieve(dbo.ID, 'S'), OperandType.Student);
                    break;
                case OperandType.Course:
                    dbo = (Course)cmd.Operand;
                    result = new DatabaseCommand(CommandType.Return, (Course)Retrieve(dbo.ID, 'C'), OperandType.Course);
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
            int i_code;
            string s_msg = "Update failed";

            switch (cmd.OperandType)
            {
                case OperandType.CatalogRequirements:
                    dbo = (CatalogRequirements)cmd.Operand;
                    i_code = Update('Y', dbo, out s_msg);
                    break;
                case OperandType.Student:
                    dbo = (Student)cmd.Operand;
                    i_code = Update('S', dbo, out s_msg);
                    break;
                case OperandType.Course:
                    dbo = (Course)cmd.Operand;
                    i_code = Update('C', dbo, out s_msg);
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
            int i_code;

            switch (cmd.OperandType)
            {
                case OperandType.CatalogRequirements:
                    dbo = (CatalogRequirements)cmd.Operand;
                    i_code = DeleteRecord('Y', dbo.ID);
                    break;
                case OperandType.Student:
                    dbo = (Student)cmd.Operand;
                    i_code = DeleteRecord('S', dbo.ID);
                    break;
                case OperandType.Course:
                    dbo = (Course)cmd.Operand;
                    i_code = DeleteRecord('C', dbo.ID);
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

            bool b_success = LoginAttempt(cred.UserName, cred.Password, out bool b_isAdmin);


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

            int i_errorCode = ChangePassword(cred.UserName, cred.Password, cred.IsActive);

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

        /// <summary>Executes a display students command.</summary>
        /// <returns>A return command containing a list of all students in the database.</returns>
        private DatabaseCommand ExecuteDisplayStudentsCommand()
        {
            List<Student> students = new List<Student>();

            try
            {
                using (IObjectContainer db = Db4oFactory.OpenFile(s_STUDENT_DB))
                {
                    IList<Student> list = db.Query(delegate (Student proto) { return proto.ID != ""; });

                    foreach (object item in list)
                    {
                        Student student = (Student)item;
                        students.Add(student);
                    } // end foreach

                    db.Close();
                } // end using
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH display students command failed. Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving list of all students failed. Msg: " + e.Message);
            } // end catch

            return new DatabaseCommand(0, "No Errors", students);
        } // end method ExecuteDisplayCommand

        /// <summary>Executes a display courses command.</summary>
        /// <returns>A return command containing a list of all courses in the database.</returns>
        private DatabaseCommand ExecuteDisplayCoursesCommand()
        {
            List<Course> courses = new List<Course>();

            try
            {
                using (IObjectContainer db = Db4oFactory.OpenFile(s_COURSE_DB))
                {
                    IList<Course> list = db.Query(delegate (Course proto) { return proto.ID != ""; });

                    foreach (object item in list)
                    {
                        Course course = (Course)item;
                        courses.Add(course);
                    } // end foreach

                    db.Close();
                } // end using
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH display courses command failed. Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving list of all courses failed. Msg: " + e.Message);
            } // end catch

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
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve salt command failed for user " + cred.UserName + ". Msg: " + e.Message);
                return new DatabaseCommand(1, "Retrieving the salt for " + cred.UserName + " failed. Msg: " + e.Message);
            } // end catch
            return new DatabaseCommand(CommandType.Return, cred);
        } // end method ExecuteGetSaltCommand

        /// <summary>The listener which waits for a command, blocking the run method.</summary>
        /// <param name="stream">Network stream to the connected client.</param>
        /// <returns>The command that is to be executed.</returns>
        private DatabaseCommand WaitForCommand(NetworkStream stream)
        {
            byte[] ba_data = new byte[BUFFER_SIZE];

            MemoryStream ms = new MemoryStream();

            // read from stream in chunks of 2048 bytes
            for (int i = 0; stream.DataAvailable; i++)
            {
                stream.Read(ba_data, i * BUFFER_SIZE, BUFFER_SIZE);
                ms.Write(ba_data, i * BUFFER_SIZE, BUFFER_SIZE);
            } // end for

            BinaryFormatter formatter = new BinaryFormatter();

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
            catch (Exception e)
            {
                WriteToLog(" -- Serialisation of return command failed. Msg: " + e.Message);
                return 1;
            } // end catch

            return cmd.ReturnCode;
        } // end method SendResult


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Management:
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
            } // end if
            else
            {
                address = IPAddress.Parse(s_IP_ADDRESS);
            } // end else

            tcpListener = new TcpListener(address, i_TCP_PORT);


            // retrieve master record from MySql db
            MySqlCommand cmd = GetCommand("-1", 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

            MySqlLock.WaitOne();
            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                ui_COL_COUNT = reader.GetUInt32(1);
                reader.Close();
                MySqlLock.ReleaseMutex();
            } // end if
            else
            {
                reader.Close();
                MySqlLock.ReleaseMutex();
                WriteToLog(" -- DBH Setup failed because the master record was not found in " + s_MYSQL_DB_NAME + "." + s_PLAN_TABLE);
                throw new Exception("Database set up failed because the master record was not found in: " + s_MYSQL_DB_NAME + "." + s_PLAN_TABLE);
            } // end else

            clientThreads = new List<Thread>();
            clients = new List<TcpClient>();

            // start listening for connection attempts
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

        /// <summary>Creates a random 256 bit salt for login credentials.</summary>
        /// <returns>A byte array filled with a random sequence of bytes.</returns>
        private byte[] GetPasswordSalt()
        {
            byte[] ba_salt = new byte[i_SALT_LENGTH];

            RNG.GetNonZeroBytes(ba_salt);

            return ba_salt;
        } // end method GetPasswordSalt

        /// <summary>Writes the given message into the DBH log with a current time stamp.</summary>
        /// <param name="s_msg">The message to log.</param>
        private static void WriteToLog(string s_msg)
        {
            // Variables:
            string s_timeStamp = DateTime.Today.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();

            LogLock.WaitOne();

            StreamWriter log = new StreamWriter(s_logFilePath, true);

            log.WriteLine(s_timeStamp + s_msg);

            log.Close();
            LogLock.ReleaseMutex();
        } // end method WriteToLog


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Website Login Handler:
        /// <summary>Checks the password entered by a user against the database record.</summary>
        /// <param name="s_ID">The username of the person attempting to log in.</param>
        /// <param name="s_pw">A string containing the user's password hash.</param>
        /// <param name="b_isAdmin">Return parameter, indicates whether the user is an admin or not.</param>
        /// <returns>True if the password entered matches the database record and out parameter b_isAdmin.</returns>
        /// <remarks>The parameter ss_pw will be destroyed during method execution, and can not be used afterwards.</remarks>
        public bool LoginAttempt(string s_ID, string s_pw, out bool b_isAdmin)
        {
            // Variables:
            var output = false;

            MySqlDataReader reader = null;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "*");

            string s_temp = String.Empty;


            b_isAdmin = false; // initialize output parameter

            try
            {
                MySqlLock.WaitOne();
                reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows) // check if user exists
                {
                    WriteToLog("-- DBH the user " + s_ID + " is attempting to login.");

                    bool b_isActive = reader.GetBoolean(5);

                    s_temp = reader.GetString(2);

                    // check if passwords match
                    if (s_pw == s_temp)
                    {
                        WriteToLog("-- DBH the user " + s_ID + " sucessfully logged in.");

                        output = true; // login sucessful
                        b_isAdmin = reader.GetBoolean(3); // get the admin value from DB
                    } // end if
                    else
                    {
                        WriteToLog("-- DBH the user " + s_ID + " entered an incorrect password, login failed.");
                    } // end else                    
                } // end if
                else
                {
                    WriteToLog("-- DBH the user " + s_ID + " does not exist, login failed.");
                } // end else
            } // end try
            finally
            {
                // end DB connection
                reader.Close();
                MySqlLock.ReleaseMutex();
            } // end finally            

            return output;
        } // end method LoginAttempt


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Database retrieve:
        /// <summary>Retrieves an object from the specified database.</summary>
        /// <param name="s_ID">The key associated with this object.</param>
        /// <param name="c_type">The type of object to retrieve.</param>
        /// <returns>The requested object.</returns>
        /// <exception cref="RetrieveError">Thrown if an invalid type is passed in arg 2.</exception>
        private object Retrieve(string s_ID, char c_type)
        {
            try
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
            } // end try
            catch (KeyNotFoundException e)
            {
                WriteToLog(" -- DBH retrieve could not find the key " + s_ID + "of type " + c_type + "Msg: " + e.Message);
                return null;
            } // end catch
        } // end method Retrieve


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // DB4O methods:
        // DB4O retrieve:
        /// <summary>Retrieves the requested object from the appropriate database.</summary>
        /// <param name="s_ID">The key of the specified object.</param>
        /// <param name="c_type">The type of object to retrieve.</param>
        /// <returns>The requested object, or null if the object was not found.</returns>
        private Database_Object RetrieveHelper(string s_ID, char c_type)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_STUDENT_DB))
                        {
                            StudentLock.WaitOne();
                            Student student = db.Query(delegate (Student proto) { return proto.ID == s_ID; })[0];
                            db.Close();
                            StudentLock.ReleaseMutex();
                            return student;
                        } // end using
                    case 'Y':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_CATALOG_DB))
                        {
                            CatalogLock.WaitOne();
                            CatalogRequirements catalog = db.Query(delegate (CatalogRequirements proto) { return proto.ID == s_ID; })[0];
                            db.Close();
                            CatalogLock.ReleaseMutex();
                            return catalog;
                        } // end using
                    case 'C':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_COURSE_DB))
                        {
                            CourseLock.WaitOne();
                            Course course = db.Query(delegate (Course proto) { return proto.ID == s_ID; })[0];
                            db.Close();
                            CourseLock.ReleaseMutex();
                            return course;
                        } // end using
                    default:
                        return null;
                } // end switch
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH retrieve encountered an exception. Type: " + c_type.ToString() + " Message: " + e.Message);
                return null;
            } // end catch
        } // end method RetrieveHelper


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // DB4O update methods:
        /// <summary>Updates the specified object by either adding it to the DB, or updating an existing object.</summary>
        /// <param name="c_type">The type of object passed in arg 2.</param>
        /// <param name="dbo">The object to update.</param>
        /// <param name="s_msg">Error message, empty if no error is encountered.</param>
        /// <returns>An error code, or 0 if update was successful.</returns>
        /// <remarks>
        ///             Return codes:
        ///             0  - success
        ///             1  - arg 2 was null 
        ///             2  - Write protection error
        ///             -1 - Unknown exception occurred
        /// </remarks>
        private int Update(char c_type, Database_Object dbo, out string s_msg)
        {
            try
            {
                UpdateHelper(c_type, dbo);
            } // end try
            catch (ArgumentNullException e)
            {
                s_msg = e.Message;
                return 1;
            } // end catch
            catch (InvalidOperationException e)
            {
                s_msg = e.Message;
                return 2;
            } // end catch
            catch (Exception e)
            {
                s_msg = e.Message;
                return -1;
            } // end catch

            s_msg = "";
            return 0;
        } // end method Update

        /// <summary>Helper method for <see cref="Update(char, Database_Object, out string)"/>.</summary>
        /// <param name="c_type">Type of object to update.</param>
        /// <param name="dbo">Object to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the object in arg 2 is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the update fails due to write protection.</exception>
        private void UpdateHelper(char c_type, Database_Object dbo)
        {
            if (dbo == null)
            {
                throw new ArgumentNullException("Update received an object that was null.");
            } // end if

            Database_Object temp = RetrieveHelper(dbo.ID, c_type);

            if (temp == null)
            {
                try
                {
                    CreateRecord(c_type, dbo);
                } // end try
                catch (Exception e)
                {
                    WriteToLog(" -- DBH an invalid create request was made. Msg: " + e.Message);
                }
            } // end if
            else
            {
                if (temp.WP == dbo.WP)
                {
                    try
                    {
                        UpdateRecord(c_type, dbo);
                    } // end try
                    catch (Exception e)
                    {
                        WriteToLog(" -- DBH an invalid update request was made. Msg: " + e.Message);
                    } // end catch
                } // end if
                else
                {
                    throw new InvalidOperationException("Write protection does not match database record.");
                } // end else
            } // end else
        } // end method UpdateHelper

        /// <summary>Updates an existing record.</summary>
        /// <param name="c_type">The type of object to update.</param>
        /// <param name="dbo">The new state of the object.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid input is passed.</exception>
        /// <remarks>
        ///          The existing object will be destroyed, the new object should be a copy of the old
        ///          object with the desired changes.
        /// </remarks>
        private void UpdateRecord(char c_type, Database_Object dbo)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_STUDENT_DB))
                        {
                            StudentLock.WaitOne();
                            Student student = db.Query(delegate (Student proto) { return proto.ID == dbo.ID; })[0];
                            db.Delete(student);
                            Student s = (Student)dbo; // cast to correct type to avoid slicing
                            s.ObjectAltered();
                            db.Store(s);
                            db.Commit();
                            db.Close();
                            StudentLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'Y':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_CATALOG_DB))
                        {
                            CatalogLock.WaitOne();
                            CatalogRequirements catalog = db.Query(delegate (CatalogRequirements proto) { return proto.ID == dbo.ID; })[0];
                            db.Delete(catalog);
                            CatalogRequirements y = (CatalogRequirements)dbo; // cast to correct type to avoid slicing
                            y.ObjectAltered();
                            db.Store(y);
                            db.Commit();
                            db.Close();
                            CatalogLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'C':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_COURSE_DB))
                        {
                            CourseLock.WaitOne();
                            Course course = db.Query(delegate (Course proto) { return proto.ID == dbo.ID; })[0];
                            db.Delete(course);
                            Course c = (Course)dbo; // cast to correct type to avoid slicing
                            c.ObjectAltered();
                            db.Store(c);
                            db.Commit();
                            db.Close();
                            CourseLock.ReleaseMutex();
                        } // end using
                        break;
                    default:
                        throw new ArgumentException("Invalid type received: " + c_type.ToString());
                } // end switch
            } // end try
            catch (Exception e)
            {
                throw new ArgumentException("Invalid input received by UpdateRecord. Msg: " + e.Message);
            } // end catch
        } // end method UpdateRecord

        /// <summary>Creates a new record for the specified object.</summary>
        /// <param name="c_type">The type of object being passed.</param>
        /// <param name="dbo">The object to store.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid input is passed.</exception>
        private void CreateRecord(char c_type, Database_Object dbo)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_STUDENT_DB))
                        {
                            StudentLock.WaitOne();
                            Student s = (Student)dbo;
                            s.ObjectAltered();
                            db.Store(s);
                            db.Commit();
                            db.Close();
                            StudentLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'Y':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_CATALOG_DB))
                        {
                            CatalogLock.WaitOne();
                            CatalogRequirements c = (CatalogRequirements)dbo;
                            c.ObjectAltered();
                            db.Store(c);
                            db.Commit();
                            db.Close();
                            CatalogLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'C':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_COURSE_DB))
                        {
                            CourseLock.WaitOne();
                            Course c = (Course)dbo;
                            c.ObjectAltered();
                            db.Store(c);
                            db.Commit();
                            db.Close();
                            CourseLock.ReleaseMutex();
                        } // end using
                        break;
                    default:
                        throw new ArgumentException("Invalid type received: " + c_type.ToString());
                } // end switch
            } // end try
            catch (Exception e)
            {
                throw new ArgumentException("Invalid input received by CreateeRecord. Msg: " + e.Message);
            } // end catch
        } // end method CreateRecord


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // DB4O delete:
        /// <summary>Deletes the object with specified ID from the appropriate DB4O database.</summary>
        /// <param name="c_type">The type of object to delete.</param>
        /// <param name="s_ID">The ID of the object to delete.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        private int DeleteRecord(char c_type, string s_ID)
        {
            try
            {
                switch (c_type)
                {
                    case 'S':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_STUDENT_DB))
                        {
                            StudentLock.WaitOne();
                            Student student = db.Query(delegate (Student proto) { return proto.ID == s_ID; })[0];
                            db.Delete(student);
                            db.Commit();
                            db.Close();
                            StudentLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'Y':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_CATALOG_DB))
                        {
                            CatalogLock.WaitOne();
                            CatalogRequirements catalog = db.Query(delegate (CatalogRequirements proto) { return proto.ID == s_ID; })[0];
                            db.Delete(catalog);
                            db.Commit();
                            db.Close();
                            CatalogLock.ReleaseMutex();
                        } // end using
                        break;
                    case 'C':
                        using (IObjectContainer db = Db4oFactory.OpenFile(s_COURSE_DB))
                        {
                            CourseLock.WaitOne();
                            Course course = db.Query(delegate (Course proto) { return proto.ID == s_ID; })[0];
                            db.Delete(course);
                            db.Commit();
                            db.Close();
                            CourseLock.ReleaseMutex();
                        } // end using
                        break;
                    default:
                        throw new ArgumentException("Invalid type received.");
                } // end switch
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH delete failed of DB4O object of type " + c_type + ". Msg: " + e.Message);
                return 1;
            } // end catch

            return 0;
        } // end method DeleteRecord


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // MySQL methods:
        // MySQL retrieve methods:
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

            MySqlDataReader reader;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

            MySqlLock.WaitOne();

            reader = cmd.ExecuteReader();
            reader.Read();

            try
            {
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
            catch (Exception e)
            {
                throw e;
            } // end catch
            finally
            {
                reader.Close();
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

            MySqlDataReader reader;

            MySqlCommand cmd = GetCommand(s_ID, 'S', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "*");

            MySqlLock.WaitOne();

            reader = cmd.ExecuteReader();
            reader.Read();

            try
            {
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
                    reader.Close();
                    throw new KeyNotFoundException(s_ID);
                } // end else
            } // end try
            catch (Exception e)
            {
                throw e;
            } // end catch
            finally
            {
                reader.Close();
                MySqlLock.ReleaseMutex();
            } // end finally

            return credentials;
        } // end method RetrieveUserCredentials


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // MySQL update methods:
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
                MySqlCommand cmd = GetCommand(plan.StudentID, 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();


                reader.Read();

                if (reader.HasRows)
                {
                    uint ui_WP = reader.GetUInt32(1);

                    reader.Close();

                    if (ui_WP != plan.WP)
                    {
                        WriteToLog(" -- DBH update failed for graduation plan of  " + plan.StudentID + " because of write protection.");
                        return 1;
                    } // end if

                    ui_WP++;

                    // update write protection
                    MySqlCommand temp = GetCommand(plan.StudentID, 'U', s_PLAN_TABLE, s_PLAN_KEY, "WP", ui_WP.ToString());
                    temp.ExecuteNonQuery();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT < plan.Classes.Length)
                    {
                        int k = plan.Classes.Length - (int)ui_COL_COUNT; // number of columns that must be added
                        AddColumns(k);
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

                    WriteToLog(" -- DBH successfully updated the plan of " + plan.StudentID + ".");
                } // end if
                else
                {
                    reader.Close();

                    // ensure the plan will fit into the table
                    if (ui_COL_COUNT < plan.Classes.Length)
                    {
                        int k = plan.Classes.Length - (int)ui_COL_COUNT; // number of columns that must be added
                        AddColumns(k);
                    } // end if

                    MySqlCommand command = GetCommand(plan.StudentID, 'I', s_PLAN_TABLE, s_PLAN_KEY, "", GetInsertValues(plan));
                    command.ExecuteNonQuery();

                    WriteToLog(" -- DBH successfully created the plan for " + plan.StudentID + ".");
                } // end else
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the student plan with ID: " + plan.StudentID + " Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return 0;
        } // end method Update

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

                    if (ui_WP != credentials.WP)
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
            catch (Exception e)
            {
                WriteToLog(" -- DBH update failed for the user credentials with username: " + credentials.UserName + " Msg: " + e.Message);
                output = 1;
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method Update

        /// <summary>Creates a new user with the specified properties.</summary>
        /// <param name="credentials">Should contain the username, and isAdmin fields. Others will be ignored.</param>
        /// <returns>True if creation was successful, false otherwise.</returns>
        /// <remarks>The database is already locked upon entering this method, thus, it does not lock the database itself.</remarks>
        private int CreateUser(Credentials credentials)
        {
            var output = 1;

            for (int i = 0; i < 10; i++) // try 10 times to ensure the salt is not the problem
            {
                credentials.PWSalt = GetPasswordSalt(); // assign new salt

                MySqlCommand cmd = GetCommand(credentials.UserName, 'I', s_CREDENTIALS_TABLE, s_CREDENTIALS_KEY, "", GetInsertValues(credentials));

                try
                {
                    cmd.ExecuteNonQuery(); // create record
                    output = 0;
                    break;
                } // end try
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

            MySqlLock.WaitOne();

            try
            {
                cmd.ExecuteNonQuery();
                output = 0;
                ChangeUserStatus(s_ID, b_activeStatus);
            } // end try
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

        // MySQL delete methods:
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
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the user " + cred.UserName + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord

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
            catch (Exception e)
            {
                WriteToLog(" -- DBH could not delete the plan belonging to " + plan.StudentID + ". Msg: " + e.Message);
            } // end catch
            finally
            {
                MySqlLock.ReleaseMutex();
            } // end finally

            return output;
        } // end method DeleteRecord


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // MySQL query generator methods:
        /// <summary>Creates an insert query based on the input.</summary>
        /// <param name="s_table">Table to insert to.</param>
        /// <param name="s_values">Values for the new row.</param>
        /// <returns>A ready to go insert sql query.</returns>
        private string GetInsertQuery(string s_table, string s_values)
        {
            //"INSERT INTO test_db.student_plans\nVALUES(\"22345678\", 1, \"Winter14\", \"CS101,CS110,GE1,UNIV101\")"

            string query = "INSERT INTO ";
            query += s_MYSQL_DB_NAME;
            query += ".";
            query += s_table;
            query += "\n VALUES(";
            query += s_values;
            query += ");";

            return query;
        } // end method GetInsertQuery

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
            if (s_columnToUpdate != "WP" && s_columnToUpdate != "admin" && s_columnToUpdate != "password")
            {
                query += "\"";
                query += s_newValue;
                query += "\"";
            }
            else
            {
                query += s_newValue;
            }
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
            query += "WHERE " + s_keyType + " = " + s_keyValue;

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


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // MySQL Database control methods:
        /// <summary>Connects to the MySQL database and initializes the class field DB_CONNECTION.</summary>
        /// <param name="s_pw">The password for the MySQL database.</param>
        /// <returns>True if login was successful, otherwise false.</returns>
        private bool ConnectToDB(ref string s_pw)
        {
            // Variables:
            string s_connStr = "server=" + s_MYSQL_DB_SERVER + ";port=" + s_MYSQL_DB_PORT + ";database=" +
                               s_MYSQL_DB_NAME + ";user id=" + s_MYSQL_DB_USER_ID + ";password=" + s_pw +
                               ";persistsecurityinfo=True;";

            //"server=<TBD>;port=<TBD>;database=<DB>;user id=<TBD>;password=<TBD>;persistsecurityinfo=True;"
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
        } // end method AttemptReconnect

        /// <summary>Keeps the DB_CONNECTION alive by accessing the DB every 2 minutes</summary>
        private void KeepAlive()
        {
            for (; ; )
            {
                MySqlCommand cmd = GetCommand("-1", 'S', s_PLAN_TABLE, s_PLAN_KEY, "*");

                MySqlLock.WaitOne();

                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Close();

                MySqlLock.ReleaseMutex();

                Console.WriteLine("Still Alive, sleeping 120000 ms).");

                Thread.Sleep(120000);
            } // end for
        } // end method KeepAlive


        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        /// <summary>Adds k columns to the student_plans table and updates the master record accordingly.</summary>
        /// <param name="k">Number of columns to add.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool AddColumns(int k)
        {
            // Variables:
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = DB_CONNECTION
            };

            var output = true;

            int i = 0; // for error output in catch


            try
            {
                for (i = 0; i < k; i++)
                {
                    cmd.CommandText = "ALTER TABLE " + s_MYSQL_DB_NAME + "." + s_PLAN_TABLE + "\nADD COLUMN qtr_" + ui_COL_COUNT + " VARCHAR(45) NULL DEFAULT '\"\"';";
                    cmd.ExecuteNonQuery();
                    ui_COL_COUNT++;
                } // end for
                WriteToLog(" -- DBH add columns successfully added " + k.ToString() + " columns to the student_plans table. The table now has "
                            + ui_COL_COUNT.ToString() + " plan columns.");
            } // end try
            catch (Exception e)
            {
                WriteToLog(" -- DBH add columns failed. The attempt to add " + k.ToString() + "columns to the plan table failed. Msg: " + e.Message);
                WriteToLog(" -- DBH add columns failed. " + (i - 1).ToString() + " columns were successfully added before failing.");
                output = false;
            } // end catch

            MySqlCommand temp = GetCommand("-1", 'U', "", ""); // get cmd for updating master record
            temp.ExecuteNonQuery(); // update master record with new column number

            return output;
        } // end method AddColumns
    } // end Class DatabaseHandler
} // end Namespace Database_Handler