﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Database_Object_Classes
{
    /// <summary>This class stores the catalog requirements for a given year.</summary>
    [Serializable]
    public class CatalogRequirements : Database_Object
    {
        // Class fields:
        /// <summary>Number of quarters a student must be enrolled at CWU before graduating.</summary>
        private uint                      ui_minQuartersAtCWU;
        
        /// <summary>Minimum cumulative GPA a student must have to graduate.</summary>
        private double                    d_minCumulativeGPA;

        /// <summary>CatalogCreditRequirements structure with credit requirements for this catalog.</summary>
        private CatalogCreditRequirements ccr_creditRequirements;

        /// <summary>List containing the individual requirements for all degrees.</summary>
        private List<DegreeRequirements>  l_degreeRequirements;

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
            this.ui_minQuartersAtCWU    = ui_minQuartersAtCWU;
            this.d_minCumulativeGPA     = d_minCumulativeGPA;
            this.ccr_creditRequirements = new CatalogCreditRequirements(ccr_creditRequirements);
            this.l_degreeRequirements   = new List<DegreeRequirements>(l_degreeRequirements);
        } // end Constructor

        /// <summary>Copy Constructor</summary>
        /// <param name="other">Object to copy</param>
        public CatalogRequirements(CatalogRequirements other) : base(other.ID)
        {
            ui_minQuartersAtCWU = other.ui_minQuartersAtCWU;
            d_minCumulativeGPA = other.d_minCumulativeGPA;
            ccr_creditRequirements = new CatalogCreditRequirements(other.ccr_creditRequirements);
            l_degreeRequirements = new List<DegreeRequirements>(other.l_degreeRequirements);
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for the minimum required quarters taken at CWU.</summary>
        public uint MinQuartersAtCWU
        {
            get => ui_minQuartersAtCWU;
            set => ui_minQuartersAtCWU = value;
        } // end MinQuartersAtCWU

        /// <summary>Getter/Setter for the minimum cumulative GPA a student must have.</summary>
        public double MinCumulativeGPA
        {
            get => d_minCumulativeGPA;
            set => d_minCumulativeGPA = value;
        } // end MinCumulativeGPA

        /// <summary>Getter/Setter for the credit requirements for this catalog year.</summary>
        public CatalogCreditRequirements CreditRequirements
        {
            get => ccr_creditRequirements;
            set => ccr_creditRequirements = new CatalogCreditRequirements(value);
        } // end CreditRequirements

        /// <summary>Getter/Setter for the list of degrees offered, which contain their own requirements.</summary>
        public ReadOnlyCollection<DegreeRequirements> DegreeRequirements
        {
            get => l_degreeRequirements.AsReadOnly();
            set => l_degreeRequirements = new List<DegreeRequirements>(value);
        } // end DegreeRequirements
    } // end Class CatalogRequirements
} // end Namespace Database_Object_Classes