﻿namespace Database_Object_Classes
{

    public abstract class Database_Object
    {
        // Class fields:
        /// <summary>Write protection to maintain data integrity of this object.</summary>
        private uint    ui_writeProtect;

        /// <summary>The unique identifier of this object.</summary>
        private string s_ID;

        
        // Getters/Setters:
        /// <summary>Getter for the write protect value of this object.</summary>
        public uint WP
        {
            get
            {
                return ui_writeProtect;
            } // end get 
        } // end WP

        /// <summary>Getter/Setter for the ID of this object. </summary>
        public string ID
        {
            get
            {
                return s_ID;
            } // end get 
            protected set
            {
                s_ID = value;

                objectAltered();
            } // end set 
        } // end ID


        // Constructor:
        /// <summary>Default Constructor. Initializes the write protect of this object to default.</summary>
        public Database_Object(string s_ID)
        {
            ui_writeProtect = 0;

            ID = s_ID;
        } // end Default Constructor


        // Methods:
        /// <summary>Updates the value of write protect of this object.</summary>
        /// <remarks>If the uint.Max value is reached, the overflow will be ignored, and the write protect reset to 0.</remarks>
        protected void objectAltered()
        {
            // overflow will simply reset the counter to 0
            unchecked
            {
                ui_writeProtect++;
            } // end unchecked
        } // end method objectAltered
    } // end Class Database_Object
} // end Namespace Database_Object_Classes
