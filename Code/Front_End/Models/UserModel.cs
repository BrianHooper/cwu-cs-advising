using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database_Object_Classes;

namespace CwuAdvising.Models
{
    /// <summary>
    /// Front-end model for database users
    /// </summary>
    public class UserModel
    {
        //Constructors: 
        /// <summary>Constructor for UserModel</summary>
        /// <param name="uName">Username for the user</param>
        /// <param name="admin">Boolean flag, true if the user is an admin</param>
        /// <param name="resetFlag">Reset flag, true if the users password should be reset</param>
        /// <param name="active">Active flag, true if the user is active</param>
        public UserModel(string uName, bool admin, bool resetFlag, bool active)
        {
            this.Username = uName;
            this.Admin = admin;
            this.ResetPassword = resetFlag;
            this.IsActive = active;
        }

        /// <summary>The user's ID, must be unique</summary>
        public string Username { get; set; }

        /// <summary>True if the user has administrative rights</summary>
        public bool Admin { get; set; }

        /// <summary>Flag to force a password reset</summary>
        public bool ResetPassword { get; set; }

        /// <summary>True if the user is active</summary>
        public bool IsActive { get; set; }
        
        /// <summary>Explicit cast operator for UserModel to Credentials conversion.</summary>
        /// <returns>A Credentials object equivalent to the given UserModel.</returns>
        public static explicit operator Credentials(UserModel model)
        {
            return new Credentials(model.Username, 0, model.Admin, model.IsActive, new byte[] { 0x20 }, string.Empty);
        }
    }
}
