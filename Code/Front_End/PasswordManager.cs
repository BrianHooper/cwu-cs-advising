using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Database_Handler;
using System.Security;
using Database_Object_Classes;
using System.Text;

namespace CwuAdvising
{
    /// <summary>Manages Password interactions.</summary>
    public class PasswordManager
    {
        private static uint ui_SALT_LENGTH = 32;
        private static uint ui_HASH_LENGTH = 64;

        /// <summary>Processes a login request.</summary>
        /// <param name="s_username">The username.</param>
        /// <param name="s_insecure_pw">The password of the user.</param>
        /// <returns>0 on successful advisor login, 1 if the advisor must change their password; 2 on successful admin login, 3 if the admin must change their password; -1 on failure, and -2 if no database is connected.</returns>
        public static int LoginAttempt(string s_username, string s_insecure_pw)
        {
            if(Program.Database.connected)
            {
                Credentials user_credentials = Program.Database.RetrieveSalt(new Credentials(s_username, 0, false, false, new byte[32], ""));

                byte[] ba_salt = new byte[ui_SALT_LENGTH];
                Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(s_insecure_pw, ba_salt, DatabaseHandler.i_HASH_ITERATIONS);

                s_insecure_pw = null;
                GC.Collect();

                byte[] ba_password_hash = hasher.GetBytes(64);

                hasher.Dispose();

                Credentials cred = new Credentials(s_username, 0, false, false, null, "") { Password_Hash = ba_password_hash };

                bool b_success = Program.Database.Login(cred);

                for(int i = 0; i < ui_HASH_LENGTH; i++)
                {
                    ba_password_hash[i] = Byte.MaxValue;
                } // end for

                ba_password_hash = null;

                GC.Collect();

                if(b_success)
                {
                    Credentials user = Program.Database.RetrieveRecord(new Credentials(s_username, 0, false, false, new byte[32], ""));

                    if(user.IsAdmin)
                    {
                        if(!user.IsActive)
                        {
                            return 3;
                        } // end if

                        return 2;
                    } // end if

                    if(!user.IsActive)
                    {
                        return 1; // The login was successful, but the user must change their password
                    } // end if

                    return 0; // The login was successful
                } // end if

                return -1; // The provided password did not match the record
            } // end if

            return -2; // The database is not connected
        } // end method LoginAttempt

        /// <summary>Changes the given user's password.</summary>
        /// <param name="s_username">The username associated with the new password.</param>
        /// <param name="s_insecure_pw">The new password.</param>
        /// <returns>True if the password was successfully changed, false otherwise</returns>
        public static bool ChangePassword(string s_username, string s_insecure_pw)
        {
            if(Program.Database.connected)
            {
                Credentials user_credentials = Program.Database.RetrieveSalt(new Credentials(s_username, 0, false, false, new byte[32], ""));

                byte[] ba_salt = new byte[ui_SALT_LENGTH];
                Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(s_insecure_pw, ba_salt, DatabaseHandler.i_HASH_ITERATIONS);

                s_insecure_pw = null;
                GC.Collect();

                byte[] ba_password_hash = hasher.GetBytes(64);

                hasher.Dispose();

                Credentials cred = new Credentials(s_username, 0, false, false, null, "") { Password_Hash = ba_password_hash };

                bool b_success = Program.Database.UpdatePassword(cred);

                return b_success;

            } // end if 

            return false;
        } // end method ChangePassword
    } // end Class PasswordManager
} // end namespace CwuAdvising
