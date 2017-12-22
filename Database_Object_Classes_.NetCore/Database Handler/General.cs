using System;

namespace Database_Handler
{
    public class RetrieveError : Exception
    {
        private char c_type;

        public RetrieveError(string msg, char type) : base(msg) => c_type = type;

        public char Type => c_type;
    };
}
