using System.Collections.Generic;

namespace Database_Object_Classes
{
    /// <summary>This class stores the catalog requirements for a given year.</summary>
    public class CatalogRequirements : Database_Object
    {
        // Class fields:
        /// <summary>Number of quarters a student must be enrolled at CWU before graduating.</summary>
        private uint ui_minQuartersAtCWU;
        
        /// <summary>Minimum cumulative GPA a student must have to graduate.</summary>
        private double d_minCumulativeGPA;

        /// <summary>CatalogCreditRequirements structure with credit requirements for this catalog.</summary>
        private CatalogCreditRequirements ccr_creditRequirements;

        /// <summary>List containing the individual requirements for all degrees.</summary>
        private List<DegreeRequirements> l_degreeRequirements;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor.</summary>
        /// <param name="s_ID">This Catalog's unique ID.</param>
        /// <param name="ui_minQuartersAtCWU">Number of quarters a student must be enrolled at CWU before graduating.</param>
        /// <param name="d_minCumulativeGPA">Minimum cumulative GPA a student must have to graduate.</param>
        /// <param name="ccr_creditRequirements">CatalogCreditRequirements structure with credit requirements for this catalog.</param>
        /// <param name="l_degreeRequirements">List of DegreeRequirements structures for this catalog.</param>
        public CatalogRequirements(string s_ID, uint ui_minQuartersAtCWU, double d_minCumulativeGPA, CatalogCreditRequirements ccr_creditRequirements, 
                                   List<DegreeRequirements> l_degreeRequirements) : base(s_ID)
        {
            this.ui_minQuartersAtCWU = ui_minQuartersAtCWU;
            this.d_minCumulativeGPA = d_minCumulativeGPA;
            this.ccr_creditRequirements = new CatalogCreditRequirements(ccr_creditRequirements);
            this.l_degreeRequirements = new List<DegreeRequirements>(l_degreeRequirements);
        } // end Constructor
    } // end Class CatalogRequirements
} // end Namespace Database_Object_Classes
