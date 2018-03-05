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
        private static uint ui_HASH_LENGTH = 64;

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
                    //Credentials user_credentials = new Credentials(s_username, 0, false, false, new byte[32], "");

                    byte[] ba_salt = new byte[DatabaseHandler.i_SALT_LENGTH];
                    Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                    /*
                    ba_salt[0] = Convert.ToByte("0x66", 16);
                    ba_salt[1] = Convert.ToByte("0x5e", 16);
                    ba_salt[2] = Convert.ToByte("0x6a", 16);
                    ba_salt[3] = Convert.ToByte("0x86", 16);
                    ba_salt[4] = Convert.ToByte("0xeb", 16);
                    ba_salt[5] = Convert.ToByte("0xc3", 16);
                    ba_salt[6] = Convert.ToByte("0x5c", 16);
                    ba_salt[7] = Convert.ToByte("0x1e", 16);
                    ba_salt[8] = Convert.ToByte("0x81", 16);
                    ba_salt[9] = Convert.ToByte("0x78", 16);
                    ba_salt[10] = Convert.ToByte("0x19", 16);
                    ba_salt[11] = Convert.ToByte("0x4c", 16);
                    ba_salt[12] = Convert.ToByte("0x87", 16);
                    ba_salt[13] = Convert.ToByte("0x72", 16);
                    ba_salt[14] = Convert.ToByte("0xe9", 16);
                    ba_salt[15] = Convert.ToByte("0x14", 16);
                    ba_salt[16] = Convert.ToByte("0xaa", 16);
                    ba_salt[17] = Convert.ToByte("0x4d", 16);
                    ba_salt[18] = Convert.ToByte("0x66", 16);
                    ba_salt[19] = Convert.ToByte("0x4f", 16);
                    ba_salt[20] = Convert.ToByte("0xea", 16);
                    ba_salt[21] = Convert.ToByte("0xe9", 16);
                    ba_salt[22] = Convert.ToByte("0xd0", 16);
                    ba_salt[23] = Convert.ToByte("0xa1", 16);
                    ba_salt[24] = Convert.ToByte("0x97", 16);
                    ba_salt[25] = Convert.ToByte("0xf6", 16);
                    ba_salt[26] = Convert.ToByte("0xb6", 16);
                    ba_salt[27] = Convert.ToByte("0x49", 16);
                    ba_salt[28] = Convert.ToByte("0x7a", 16);
                    ba_salt[29] = Convert.ToByte("0xfd", 16);
                    ba_salt[30] = Convert.ToByte("0x9e", 16);
                    ba_salt[31] = Convert.ToByte("0xa8", 16);
                    */
                    // 62-A8-C5-07-7C-80-C4-C3-0C-7D-C6-04-3B-96-6F-A8-04-26-67-DA-EE-22-1C-1B-89-B4-18-E9-7F-2D-CB-B2-8D-42-B3-6D-86-E4-0A-D0-00-E8-9F-29-4D-06-EB-1E-AF-87-48-40-F0-06-A5-DB-8E-54-1B-B2-7D-C0-9B-73
                    // 62-AB-C5-07-7C-80-C4-C3-0C-7D-C6-04-3B-96-6F-A8-04-26-67-DA-EE-22-1C-1B-89-B4-18-E9-7F-2D-CB-B2-8D-42-B3-6D-86-E4-0A-D0-00-E8-9F-29-4D-06-EB-1E-AF-87-48-40-F0-06-A5-DB-8E-54-1B-B2-7D-C0-9B-73
                    // 62-A8-C5-07-7C-80-C4-C3-0C-7D-C6-04-3B-96-6F-A8-04-26-67-DA-EE-22-1C-1B-89-B4-18-E9-7F-2D-CB-B2-8D-42-B3-6D-86-E4-0A-D0-00-E8-9F-29-4D-06-EB-1E-AF-87-48-40-F0-06-A5-DB-8E-54-1B-B2-7D-C0-9B-73 
                    // 62-A8-C5-07-7C-80-C4-C3-0C-7D-C6-04-3B-96-6F-A8-04-26-67-DA-EE-22-1C-1B-89-B4-18-E9-7F-2D-CB-B2-8D-42-B3-6D-86-E4-0A-D0-00-E8-9F-29-4D-06-EB-1E-AF-87-48-40-F0-06-A5-DB-8E-54-1B-B2-7D-C0-9B-73
                    // 40-5D-21-14-BA-63-57-AA-E3-3B-7A-F6-FF-50-48-BA-CB-81-D0-BF-D3-D9-49-5C-8A-59-F4-18-32-3A-1D-E9-C5-FC-F2-66-F6-3A-3B-00-E0-DD-22-8A-2D-20-58-0D-86-92-35-B7-8F-BF-E3-2D-3A-90-89-F3-04-86-91-B4

                    Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(s_insecure_pw, ba_salt, DatabaseHandler.i_HASH_ITERATIONS);


                    byte[] ba_password_hash = hasher.GetBytes(64);

                    //string hash = BitConverter.ToString(ba_password_hash);

                    ////IndexModel.LoginErrorMessage = "The hash for the password " + s_insecure_pw + " is " + hash;
                    //DatabaseInterface.WriteToLog("The has for password " + s_insecure_pw + " is " + hash);
                    ////return -2;


                    //s_insecure_pw = null;
                    //hasher.Dispose();
                    //GC.Collect();


                    Credentials cred = new Credentials(s_username, 0, false, false, new byte[DatabaseHandler.i_SALT_LENGTH], "") { Password_Hash = ba_password_hash };


                    bool b_success = Program.Database.Login(cred);

                    //for(int i = 0; i < ui_HASH_LENGTH; i++)
                    //{
                    //    ba_password_hash[i] = Byte.MaxValue;
                    //} // end for

                    //ba_password_hash = null;

                    //GC.Collect();

                    if (b_success)
                    {
                        Credentials user = Program.Database.RetrieveRecord(new Credentials(s_username, 0, false, false, new byte[32], ""));

                        if (user.IsAdmin)
                        {
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
