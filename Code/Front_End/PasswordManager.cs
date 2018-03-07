using System;
using System.Security.Cryptography;
using Database_Handler;
using Database_Object_Classes;
using CwuAdvising.Pages;

namespace CwuAdvising
{
    /// <summary>Manages Password interactions.</summary>
    public class PasswordManager
    {
        private static uint ui_SALT_LENGTH = 32;
        private static int i_HASH_LENGTH = 64;

        /// <summary>Processes a login request.</summary>
        /// <param name="s_username">The username.</param>
        /// <param name="s_insecure_pw">The password of the user.</param>
        /// <returns>0 on successful advisor login, 1 if the advisor must change their password; 2 on successful admin login, 3 if the admin must change their password; -1 on failure, and -2 if no database is connected.</returns>
        public static int LoginAttempt(string s_username, string s_insecure_pw)
        {
            try
            {
                if (true || Program.Database.connected)
                {
                    Credentials user_credentials = Program.Database.RetrieveSalt(new Credentials(s_username, 0, false, false, new byte[32], ""));

                    byte[] ba_salt = new byte[DatabaseHandler.i_SALT_LENGTH];
                    Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                    Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(s_insecure_pw, ba_salt, DatabaseHandler.i_HASH_ITERATIONS);


                    byte[] ba_password_hash = hasher.GetBytes(i_HASH_LENGTH);

                    hasher.Dispose();

                    Credentials cred = new Credentials(s_username, 0, false, false, new byte[DatabaseHandler.i_SALT_LENGTH], "") { Password_Hash = ba_password_hash };


                    bool b_success = Program.Database.Login(cred);

                    if (b_success)
                    {
                        Credentials user = Program.Database.RetrieveRecord(new Credentials(s_username, 0, false, false, new byte[32], ""));
                        IndexModel.CurrentUser = s_username;
                        IndexModel.LoggedIn = true;
                        if (user.IsAdmin)
                        {
                            IndexModel.Administrator = true;
                            if (!user.IsActive)
                            {
                                DatabaseInterface.WriteToLog("Administrator " + s_username + " signed in, must change pw.");
                                return 3;
                            } // end if
                            DatabaseInterface.WriteToLog("Administrator " + s_username + " signed in successfully.");

                            return 2;
                        } // end if

                        if (!user.IsActive)
                        {
                            DatabaseInterface.WriteToLog("Advisor " + s_username + " signed in, must change pw.");
                            return 1; // The login was successful, but the user must change their password
                        } // end if

                        DatabaseInterface.WriteToLog("Advisor " + s_username + " signed in successfully.");
                        return 0; // The login was successful
                    } // end if

                    DatabaseInterface.WriteToLog("User " + s_username + " did not provide corect password.");
                    return -1; // The provided password did not match the record
                } // end if
            }
            catch(Exception e)
            {
                DatabaseInterface.WriteToLog(" -- Catch block reached. Msg: " + e.Message);
            }

            DatabaseInterface.WriteToLog("Sign-in failed, no database available.");

            return -1; // The database is not connected
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

                byte[] ba_salt = new byte[DatabaseHandler.i_SALT_LENGTH];
                Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(s_insecure_pw, ba_salt, DatabaseHandler.i_HASH_ITERATIONS);

                byte[] ba_password_hash = hasher.GetBytes(i_HASH_LENGTH);

                Credentials cred = new Credentials(s_username, 0, false, false, new byte[DatabaseHandler.i_SALT_LENGTH], "") { Password_Hash = ba_password_hash };

                hasher.Dispose();

                bool b_success = Program.Database.UpdatePassword(cred);

                return b_success;
            } // end if 

            return false;
        } // end method ChangePassword
    } // end Class PasswordManager
} // end namespace CwuAdvising
