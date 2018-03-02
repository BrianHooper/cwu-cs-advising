using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Database_Object_Classes
{
    /// <summary>This class stores the catalog requirements for a given year.</summary>
    [Serializable]
    public class CatalogRequirements : Database_Object
    {
        // Class fields:
        /// <summary>List containing the individual requirements for all degrees.</summary>
        private List<DegreeRequirements>  l_degreeRequirements;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor.</summary>
        /// <param name="s_ID">This Catalog's unique ID.</param>
        /// <param name="l_degreeRequirements">List of DegreeRequirements structures for this catalog.</param>
        public CatalogRequirements(string s_ID, List<DegreeRequirements> l_degreeRequirements) : base(s_ID)
        {
            this.l_degreeRequirements   = new List<DegreeRequirements>(l_degreeRequirements);
        } // end Constructor

        /// <summary>Copy Constructor</summary>
        /// <param name="other">Object to copy</param>
        public CatalogRequirements(CatalogRequirements other) : base(other.ID)
        {
            l_degreeRequirements = new List<DegreeRequirements>(other.l_degreeRequirements);
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:        
        /// <summary>Getter/Setter for the list of degrees offered, which contain their own requirements.</summary>
        public ReadOnlyCollection<DegreeRequirements> DegreeRequirements
        {
            get => l_degreeRequirements.AsReadOnly();
            set => l_degreeRequirements = new List<DegreeRequirements>(value);
        } // end DegreeRequirements
    } // end Class CatalogRequirements
} // end Namespace Database_Object_Classes