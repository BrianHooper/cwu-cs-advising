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
        DisplayStudents,
        /// <summary>Get password salt command.</summary>
        GetSalt,
        /// <summary>Result from DBH.</summary>
        Return
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
    class DatabaseCommand
    {
        /// <summary>Type of Command to execute.</summary>
        private CommandType ct_commandType;

        /// <summary>The operand to execute the command on.</summary>
        private object o_operand;

        /// <summary>A list of students for display student command.</summary>
        private List<Student> l_students;

        /// <summary>The type of the o_operand object.</summary>
        private OperandType ot_type;

        /// <summary>Return code of a return command, will either be 0 (no error) or an error code.</summary>
        private int i_returnCode;

        /// <summary>If i_returnCode is not 0, there will be an error message contained in this variable.</summary>
        private string s_errorMsg;

        /// <summary>Constructor for DB4O commands.</summary>
        /// <param name="ct">Type of command.</param>
        /// <param name="dbo">The operand for this command.</param>
        /// <param name="ot">The type of this operand.</param>
        public DatabaseCommand(CommandType ct, Database_Object dbo, OperandType ot)
        {
            ct_commandType = ct;
            ot_type = ot;

            switch(ot)
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
        /// <param name="ct">Command type, must be set to display students.</param>
        public DatabaseCommand(CommandType ct)
        {
            ct_commandType = ct;
        } // end Constructor

        /// <summary>Constructor for Update/Delete returns containing relevant info about execution of the command.</summary>
        /// <param name="code">The error code, or 0 if execution was successful.</param>
        /// <param name="msg">A message detailing an error, "No Errors" if no issues occurred.</param>
        /// <param name="list">A list of students for the display students command.</param>
        public DatabaseCommand(int code = 0, string msg = "No Errors", List<Student> list = null)
        {
            ct_commandType = CommandType.Return;
            i_returnCode = code;
            s_errorMsg = msg;
            l_students = list;
        } // end Constructor

        /// <summary>Getter for the type of command.</summary>
        public CommandType CommandType => ct_commandType;

        /// <summary>Getter for the type of operand.</summary>
        public OperandType OperandType => ot_type;

        /// <summary>Getter for the list of students for a display command.</summary>
        public List<Student> StudentList => l_students;

        /// <summary>Getter for the operand.</summary>
        public object Operand => o_operand;
    } // end Class DatabaseCommand
} // end namespace Database_Handler
