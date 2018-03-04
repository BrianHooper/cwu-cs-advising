using System;
using System.Collections.Generic;

namespace Database_Object_Classes
{
    /// <summary>Base class for all objects in the db4o database, containing the basic construct used by all objects.</summary>
    [Serializable]
    public abstract class Database_Object 
    {
        // Class fields:
        /// <summary>Write protection to maintain data integrity of this object.</summary>
        private uint   ui_writeProtect;

        /// <summary>The unique identifier of this object.</summary>
        private string s_ID;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Default Constructor. Initializes the write protect of this object to default.</summary>
        /// <param name="s_ID">The ID of this object.</param>
        public Database_Object(string s_ID)
        {
            ui_writeProtect = 0;

            ID = string.Copy(s_ID);
        } // end Default Constructor

        /// <summary>Copy Constructor for this class.</summary>
        /// <param name="other">Object to copy.</param>
        public Database_Object(Database_Object other)
        {
            ui_writeProtect = other.ui_writeProtect;
            ID = string.Copy(other.s_ID);
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for the write protect value of this object.</summary>
        public uint WP
        {
            get => ui_writeProtect;
            set => ui_writeProtect = value;   
        }

        /// <summary>Getter/Setter for the ID of this object. </summary>
        public string ID
        {
            get => s_ID;
            set => s_ID = string.Copy(value);
        } // end ID

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>Updates the value of write protect of this object.</summary>
        /// <remarks>If the uint.Max value is reached, the overflow will be ignored, and the write protect is reset to 0.</remarks>
        public void ObjectAltered()
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
        } // end method GetHashCode

        /// <summary>Equals operator for comparing two Database Objects.</summary>
        /// <param name="obj">Operand being compared to this object.</param>
        /// <returns>True if the two objects are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            Database_Object d = (Database_Object)obj;
            return string.Equals(ID, d.ID);
        } // end method Equals
    } // end Class Database_Object
} // end Namespace Database_Object_Classes