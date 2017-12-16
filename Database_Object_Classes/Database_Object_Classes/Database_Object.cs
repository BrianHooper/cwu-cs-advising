namespace Database_Object_Classes
{
    public abstract class Database_Object
    {
        // Class fields:
        /// <summary>Write protection to maintain data integrity of this object.</summary>
        private int    i_writeProtect;

        /// <summary>The unique identifier of this object.</summary>
        private string s_ID;

        
        // Getters/Setters:
        /// <summary>Getter for the write protect value of this object.</summary>
        public int WP
        {
            get
            {
                return i_writeProtect;
            } // end get 
        } // end WP

        /// <summary>Getter/Setter for the ID of this object. </summary>
        public string ID
        {
            get
            {
                return s_ID;
            } // end get 
            set
            {
                s_ID = value;
            } // end set 
        } // end ID


        // Constructor:
        /// <summary>Default Constructor. Initializes the write protect of this object to default.</summary>
        public Database_Object(string s_ID)
        {
            i_writeProtect = 0;
            this.s_ID = s_ID;
        } // end Default Constructor


        // Methods:
        /// <summary>Updates the value of write protect of this object, either incrementing it, or resetting it to 1 if the max value is reached.</summary>
        protected void objectAltered()
        {
            if (i_writeProtect == int.MaxValue)
            {
                i_writeProtect = 1;
            } // end if
            else
            {
                i_writeProtect++;
            } // end else
        } // end method objectAltered
    } // end Class Database_Object
} // end Namespace Database_Object_Classes
