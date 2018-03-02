using Database_Object_Classes;
using System;
using System.Collections.Generic;

namespace Database_Handler
{
    /// <summary>Enum for command types.</summary>
    [Serializable]
    public enum CommandType
    {
        /// <summary>Retrieve command.</summary>
        Retrieve,
        /// <summary>Login command.</summary>
        Login,
        /// <summary>Change user password command.</summary>
        ChangePW,
        /// <summary>Update command.</summary>
        Update,
        /// <summary>Delete command.</summary>
        Delete,
        /// <summary>Display student list command.</summary>
        DisplayUsers,
        /// <summary>Display student list command.</summary>
        DisplayCatalogs,
        /// <summary>Display course list command.</summary>
        DisplayCourses,
        /// <summary>Get password salt command.</summary>
        GetSalt,
        /// <summary>Result from DBH.</summary>
        Return,
        /// <summary>Returned when an error occurred during execution of a command.</summary>
        Error,
        /// <summary>Terminates the connection to DBH, and kills the running DBH thread.</summary>
        Disconnect
    };

    /// <summary>Enum for operand types.</summary>
    [Serializable]
    public enum OperandType
    {
        /// <summary>Student operand.</summary>
        Student,
        /// <summary>Course operand.</summary>
        Course,
        /// <summary>CatalogRequirements operand.</summary>
        CatalogRequirements,
        /// <summary>PlanInfo operand.</summary>
        PlanInfo,
        /// <summary>Credentials operand.</summary>
        Credentials
    };

    /// <summary>Wrapper class for Database instructions.</summary>
    [Serializable]
    public class DatabaseCommand
    {
        /// <summary>Type of Command to execute.</summary>
        private CommandType ct_commandType;

        /// <summary>The operand to execute the command on.</summary>
        private object o_operand;

        /// <summary>A list of catalogs for display catalogs command.</summary>
        private List<CatalogRequirements> l_catalogs;

        /// <summary>A list of credentials for display users command.</summary>
        private List<Credentials> l_credentials;

        /// <summary>A list of courses for display courses command.</summary>
        private List<Course> l_courses;

        /// <summary>The type of the o_operand object.</summary>
        private OperandType ot_type;

        /// <summary>Return code of a return command, will either be 0 (no error) or an error code.</summary>
        private int i_returnCode;

        /// <summary>If i_returnCode is not 0, there will be an error message contained in this variable.</summary>
        private string s_errorMsg;

        /// <summary></summary>
        private bool b_isShallow;

        /// <summary>Constructor for DB4O commands.</summary>
        /// <param name="ct">Type of command.</param>
        /// <param name="dbo">The operand for this command.</param>
        /// <param name="ot">The type of this operand.</param>
        /// <param name="b_shallow">Whether or not to retrieve shallow copy of a Course or CatalogRequirements object, ignored for Student objects.</param>
        public DatabaseCommand(CommandType ct, Database_Object dbo, OperandType ot, bool b_shallow = false)
        {
            ct_commandType = ct;
            ot_type = ot;
            b_isShallow = b_shallow;

            switch (ot)
            {
                case OperandType.Student:
                    o_operand = new Student((Student)dbo);
                    break;
                case OperandType.Course:
                    o_operand = new Course((Course)dbo);
                    break;
                case OperandType.CatalogRequirements:
                    o_operand = new CatalogRequirements((CatalogRequirements)dbo);
                    break;
            }
        } // end Constructor

        /// <summary>Constructor for MySQL credentials commands.</summary>
        /// <param name="ct">The type of command.</param>
        /// <param name="cred">The operand credentials.</param>
        public DatabaseCommand(CommandType ct, Credentials cred)
        {
            ct_commandType = ct;
            ot_type = OperandType.Credentials;
            o_operand = new Credentials(cred);
        } // end Constructor

        /// <summary>Constructor for MySQL plan commands.</summary>
        /// <param name="ct">Type of command to execute.</param>
        /// <param name="plan">The operand plan.</param>
        public DatabaseCommand(CommandType ct, PlanInfo plan)
        {
            ct_commandType = ct;
            ot_type = OperandType.PlanInfo;
            o_operand = new PlanInfo(plan);
        } // end Constructor

        /// <summary>Constructor for creating a display students command.</summary>
        /// <param name="ct">Command type for this command.</param>
        /// <param name="b_shallow">Whether or not a display command should retrieve shallow copies of the inner course objects.</param>
        /// <remarks>
        /// The argument must be DisplayCatalogs/DisplayCourses or Disconnect.
        /// True should be passed if the inner course objects are not being used to avoid large amounts of recursion.
        /// </remarks>
        public DatabaseCommand(CommandType ct, bool b_shallow = false)
        {
            ct_commandType = ct;
        } // end Constructor

        /// <summary>Constructor for postback from DBH; contains relevant info about execution of the command.</summary>
        /// <param name="code">The error code, or 0 if execution was successful.</param>
        /// <param name="msg">A message detailing an error, "No Errors" if no issues occurred.</param>
        /// <param name="students">A list of students for the display students command.</param>
        /// <param name="courses">A list of courses for the display courses command.</param>
        /// <remarks>This command type should not be sent to DBH, it is only intended for DBH to return information to the client.</remarks>
        public DatabaseCommand(int code = 0, string msg = "No Errors", List<CatalogRequirements> catalogs = null, List<Course> courses = null, List<Credentials> credentials = null)
        {
            switch(code)
            {
                case 0:
                    ct_commandType = CommandType.Return;
                    break;
                default:
                    ct_commandType = CommandType.Error;
                    break;
            } // end switch 

            i_returnCode = code;
            s_errorMsg = msg;
            l_catalogs = catalogs;
            l_credentials = credentials;
            l_courses = courses;
        } // end Constructor

        /// <summary>Getter for the error code.</summary>
        /// <remarks>0 means normal execution, 99 means disconnect command received, and everything else is an error (specified in error message).</remarks>
        public int ReturnCode => i_returnCode;

        /// <summary>Getter for the error message.</summary>
        public string ErrorMessage => s_errorMsg;

        /// <summary>Getter for the type of command.</summary>
        public CommandType CommandType => ct_commandType;

        /// <summary>Getter for the type of operand.</summary>
        public OperandType OperandType => ot_type;

        /// <summary>Getter for the list of catalogs for a display command.</summary>
        public List<CatalogRequirements> CatalogList => l_catalogs;

        /// <summary>Getter for the list of users for a display command</summary>
        public List<Credentials> UserList => l_credentials;

        /// <summary>Getter for the list of courses for a display command.</summary>
        public List<Course> CourseList => l_courses;

        /// <summary>Getter for the operand.</summary>
        public object Operand => o_operand;

        /// <summary>Getter/Setter for whether this command contains shallow or complete objects.</summary>
        /// <remarks>This property is used only for retrieve operations, all others ignore it.</remarks>
        public bool IsShallow
        {
            get
            {
                return b_isShallow;
            }
            set
            {
                b_isShallow = value;
            }
        }

    } // end Class DatabaseCommand
} // end namespace Database_Handler
