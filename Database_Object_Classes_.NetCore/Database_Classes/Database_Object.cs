using System.Collections.Generic;

namespace Database_Object_Classes
{
    /// <summary>Base class for all objects in the db4o database, containing the basic construct used by all objects.</summary>
    public abstract class Database_Object
    {
        // Class fields:
        /// <summary>Write protection to maintain data integrity of this object.</summary>
        private uint   ui_writeProtect;

        /// <summary>The unique identifier of this object.</summary>
        private string s_ID;

        
        // Getters/Setters:
        /// <summary>Getter for the write protect value of this object.</summary>
        public uint WP => ui_writeProtect;

        /// <summary>Getter/Setter for the ID of this object. </summary>
        public string ID
        {
            get => s_ID;
            protected set
            {
                s_ID = value;

                ObjectAltered();
            } // end set 
        } // end ID


        // Constructor:
        /// <summary>Default Constructor. Initializes the write protect of this object to default.</summary>
        public Database_Object(string s_ID)
        {
            ui_writeProtect = 0;

            ID = s_ID;
        } // end Default Constructor

        /// <summary>Equals operator for comparing two Database Objects.</summary>
        /// <param name="obj">Operand being compared to this object.</param>
        /// <returns>True if the two objects are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            Database_Object d = (Database_Object)obj;
            return ID == d.ID;
        }


        // Methods:
        /// <summary>Updates the value of write protect of this object.</summary>
        /// <remarks>If the uint.Max value is reached, the overflow will be ignored, and the write protect is reset to 0.</remarks>
        protected void ObjectAltered()
        {
            // overflow will simply reset the counter to 0
            unchecked
            {
                ui_writeProtect++;
            } // end unchecked
        } // end method objectAltered

        /// <summary>Default hashing function.</summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            var hashCode = 987006414;
            hashCode = hashCode * -1521134295 + ui_writeProtect.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(s_ID);
            hashCode = hashCode * -1521134295 + WP.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
            return hashCode;
        }
    } // end Class Database_Object
} // end Namespace Database_Object_Classes