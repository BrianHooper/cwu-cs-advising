using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Database_Handler
{
    /// <summary>
    /// Exception that is thrown when the recursion depth exceeds a specified limit.
    /// </summary>
    public class RecursionDepthException : Exception
    {
        /// <summary>Constructs a new exception object.</summary>
        /// <param name="msg">The message.</param>
        public RecursionDepthException(string msg) : base(msg) { }
    } // end Class RecursionDepthException

    /// <summary>Exception class for an error that occurs while retrieving an object from a database.</summary>
    public class RetrieveError : Exception
    {
        /// <summary>Type of object that was supposed to be retrieved.</summary>
        public char Type { get; }

    /// <summary>Constructor for this exception.</summary>
    /// <param name="msg">The message.</param>
    /// <param name="type">The type of object that was supposed to be retrieved.</param>
        public RetrieveError(string msg, char type) : base(msg) => Type = type;
    }; // end Class RetrieveError

    /// <summary>Utilities class containing any utility functions needed by DBH.</summary>
    public class Utilities
    {
        /// <summary>Checks whether two secure string objects contain the same string.</summary>
        /// <param name="ss_s1">First secure string.</param>
        /// <param name="ss_s2">Second secure string</param>
        /// <returns>True if the two secure strings passed in arg1 and arg2 contain the same string.</returns>
        /// <remarks>Stolen from: https://stackoverflow.com/a/4502736/7687278 </remarks>
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

        /// <summary>Turns a secure string into a managed string.</summary>
        /// <param name="ss_string">The secure string to extract.</param>
        /// <returns>The string contained within arg1.</returns>
        /// <remarks>Stolen from: https://stackoverflow.com/a/819705/7687278 </remarks>
        public static string SecureStringToString(SecureString ss_string)
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