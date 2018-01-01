using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Database_Handler
{
    public class RetrieveError : Exception
    {
        public char Type { get; }

        public RetrieveError(string msg, char type) : base(msg) => Type = type;
    };

    public class CommandUninterpretableError : Exception
    {
        public string Command { get; }

        public CommandUninterpretableError(string message, string cmd) : base(message) => Command = cmd;
    }

    public class Utilities
    {
        public static unsafe bool SecureStringEqual(SecureString ss_s1, SecureString ss_s2)
        {
            // check if either string is null
            if (ss_s1 == null || ss_s2 == null)
            {
                return false;
            } // end if

            // check if strings are the same length
            if (ss_s1.Length != ss_s2.Length)
            {
                return false;
            } // end if

            // create pointers for the strings
            IntPtr ip_bstr1 = IntPtr.Zero;
            IntPtr ip_bstr2 = IntPtr.Zero;

            // set up unmanaged memory area
            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                // put strings into unmanaged memory
                ip_bstr1 = Marshal.SecureStringToBSTR(ss_s1);
                ip_bstr2 = Marshal.SecureStringToBSTR(ss_s2);

                unsafe // unsafe b/c using unmanaged memory
                {
                    // iterate through the strings and compare each character
                    for (Char* ptr1 = (Char*)ip_bstr1.ToPointer(), ptr2 = (Char*)ip_bstr2.ToPointer();
                        *ptr1 != 0 && *ptr2 != 0;
                         ++ptr1, ++ptr2)
                    {
                        if (*ptr1 != *ptr2)
                        {
                            return false; // strings are different in at least one position
                        } // end if
                    } // end for
                } // end unsafe

                return true;
            } // end try

            finally // zero out and deallocate the unmanaged memory
            {                
                if (ip_bstr1 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ip_bstr1);
                } // end if

                if (ip_bstr2 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ip_bstr2);
                } // end if
            } // end finally
        } // end SecureStringEqual

        public static unsafe string SecureStringToString(SecureString ss_string)
        {
            if (ss_string == null)
            {
                throw new ArgumentNullException("The string was null.");
            } // end if

            IntPtr valuePtr = IntPtr.Zero;

            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(ss_string);

                return Marshal.PtrToStringUni(valuePtr);
            } // end try
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            } // end finally
        } // end SecureStringToString
    } // end Class Utilities
} // end namespace Database_Handler