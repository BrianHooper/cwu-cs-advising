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
        /// <param name="ss_password">The password of the user.</param>
        /// <returns>0 on success, 1 on failure, -1 if no database is connected.</returns>
        public static int LoginAttempt(string s_username, SecureString ss_password)
        {
            if(Program.Database.connected)
            {
                Credentials user_credentials = Program.Database.RetrieveSalt(new Credentials(s_username, 0, false, false, new byte[32], ""));

                byte[] ba_salt = new byte[ui_SALT_LENGTH];
                Array.Copy(user_credentials.PWSalt, ba_salt, ui_SALT_LENGTH);

                string s_insecure_pw = Utilities.SecureStringToString(ss_password);                
                
                ss_password.Dispose();

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
                    return 0;
                } // end if

                return 1;
            } // end if

            return -1;
        } // end method LoginAttempt

        /// <summary>Changes the given user's password.</summary>
        /// <param name="s_username">The username associated with the new password.</param>
        /// <param name="ss_password">The new password.</param>
        /// <returns></returns>
        public static bool ChangePassword(string s_username, SecureString ss_password)
        {
            if(Program.Database.connected)
            {

            } // end if 

            return false;
        } // end method ChangePassword
    } // end Class PasswordManager
} // end namespace CwuAdvising
