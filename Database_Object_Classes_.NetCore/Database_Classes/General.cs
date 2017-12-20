using System;
using System.Collections.Generic;

namespace Database_Object_Classes
{
    /// <summary>Enum for the season when a quarter occurs (Winter, Spring, etc.).</summary>
    /// <remarks>Winter = 0, Spring = 1, Summer = 2, Fall = 3.</remarks>
    public enum Season
    {
        /// <summary>Winter quarter, denoted by 0.</summary>
        Winter,
        /// <summary>Spring quarter, denoted by 1.</summary>
        Spring,
        /// <summary>Summer quarter, denoted by 2.</summary>
        Summer,
        /// <summary>Fall quarter, denoted by 3.</summary>
        Fall
    }

    /// <summary>Structure which contains the year, and "season" of the quarter.</summary>
    public struct Quarter
    {
        private uint ui_year;
        private Season s_quarter;

        /// <summary>Returns a default Quarter object.</summary>
        public static Quarter DefaultQuarter => new Quarter(0, Season.Spring);

        /// <summary>Getter/Setter for the year member of this quarter.</summary>
        public uint Year
        {
            get => ui_year;
            set => ui_year = value;
        } // end Year

        /// <summary>Getter/Setter for the season member of this quarter.</summary>
        public Season QuarterSeason
        {
            get => s_quarter;
            set => s_quarter = value;
        } // end StartQuarter

        /// <summary>Constructor for this structure.</summary>
        /// <param name="ui_yr">The year of this quarter.</param>
        /// <param name="s_qtr">The season of this quarter.</param>
        /// <remarks>The year is an unsigned integer, and should be of the form YYYY, e.g. 2018.
        ///          The season is an integer in the range 0-3, or alternatively the type Season.
        /// </remarks>
        public Quarter(uint ui_yr, Season s_qtr)
        {
            ui_year = ui_yr;
            s_quarter = s_qtr;
        } // end Constructor

        /// <summary>Copy Constructor for this structure.</summary>
        /// <param name="q_other">The Quarter to be copied.</param>
        public Quarter(Quarter q_other)
        {
            ui_year = q_other.Year;
            s_quarter = q_other.QuarterSeason;
        }

    } // end structure Quarter

    /// <summary>Structure storing first and last name of a person.</summary>
    public struct Name
    {
        private string s_fName;
        private string s_lName;

        /// <summary>Getter/Setter for the first name of this person.</summary>
        public string First
        {
            get => s_fName;
            set => s_fName = value;
        }

        /// <summary>Getter/Setter for the last name of this person.</summary>
        public string Last
        {
            get => s_lName;
            set => s_lName = value;
        }

        /// <summary>Constructor for this structure.</summary>
        /// <param name="s_fname">First name.</param>
        /// <param name="s_lname">Last name.</param>
        public Name(string s_fname, string s_lname)
        {
            s_fName = s_fname;
            s_lName = s_lname;
        }
        
        /// <summary>Copy Constructor for this structure.</summary>
        /// <param name="n_other">The Name to be copied.</param>
        public Name(Name n_other)
        {
            s_fName = n_other.First;
            s_lName = n_other.Last;
        }

        /// <summary>Returns this name as a string.</summary>
        /// <returns>The name in the format FirstName LastName.</returns>
        public override string ToString()
        {
            return s_fName + " " +  s_lName;
        }

    } // end structure Name

    /// <summary>Structure storing the credit requirements for a catalog.</summary>
    public struct CatalogCreditRequirements
    {
        private uint ui_minCredits;
        private uint ui_maxCreditsTransfer;
        private uint ui_maxCreditsTransferLower;
        private uint ui_minCreditsUpper;
        private uint ui_minCreditsResidency;
        private uint ui_maxCreditsChallenge;
        private uint ui_minCreditsEarnedForMajor;
        private uint ui_maxCreditsCoOp290;
        private uint ui_maxCreditsCoOp;
        private uint ui_maxCreditsCoOpTransfer;
        private uint ui_maxCreditsCoOpGrad;

        /// <summary>Minimum credits to graduate with Bachelor's degree.</summary>
        public uint MinimumCredits
        {
            get => ui_minCredits;
            set => ui_minCredits = value;
        }
        /// <summary>Maximum transferable credits.</summary>
        public uint MaximumTransferCredits
        {
            get => ui_maxCreditsTransfer;
            set => ui_maxCreditsTransfer = value;
        }
        /// <summary>Maximum transferable LD credits.</summary>
        public uint MaximumTransferCreditsLD
        {
            get => ui_maxCreditsTransferLower;
            set => ui_maxCreditsTransferLower = value;
        }
        /// <summary>Minimum UD credits to graduate.</summary>
        public uint MinimumCreditsUD
        {
            get => ui_minCreditsUpper;
            set => ui_minCreditsUpper = value;
        }
        /// <summary>Minimum credits taken at CWU.</summary>
        public uint MinimumCreditsForResidency
        {
            get => ui_minCreditsResidency;
            set => ui_minCreditsResidency = value;
        }
        /// <summary>Maximum credits from course challenges.</summary>
        public uint MaximumChallengeCredits
        {
            get => ui_maxCreditsChallenge;
            set => ui_maxCreditsChallenge = value;
        }
        /// <summary>Minimum credits earned in major field by non-transfer students.</summary>
        public uint MinimumCWUCreditsMajor
        {
            get => ui_minCreditsEarnedForMajor;
            set => ui_minCreditsEarnedForMajor = value;
        }
        /// <summary>Naximum coop credits allowed at the 290 level.</summary>
        public uint MaximumCoOpCredits290Level
        {
            get => ui_maxCreditsCoOp290;
            set => ui_maxCreditsCoOp290 = value;
        }
        /// <summary>Naximum coop credits allowed.</summary>
        public uint MaximumCoOpCredits
        {
            get => ui_maxCreditsCoOp;
            set => ui_maxCreditsCoOp = value;
        }
        /// <summary>Naximum coop credits allowed for transfer students.</summary>
        public uint MaximumCoOpCreditsTransfer
        {
            get => ui_maxCreditsCoOpTransfer;
            set => ui_maxCreditsCoOpTransfer = value;
        }
        /// <summary>Naximum coop credits allowed for graduate programs.</summary>
        public uint MaximumCoOpCreditsGraduateProgram
        {
            get => ui_maxCreditsCoOpGrad;
            set => ui_maxCreditsCoOpGrad = value;
        }

        /// <summary>Constructor for this structure.</summary>
        /// <param name="ui_minCredits">Minimum Credits for Bachelor's Degree.</param>
        /// <param name="ui_maxCreditsTransfer">Maximum acceptable credits transfered from another institution.</param>
        /// <param name="ui_maxCreditsTransferLower">Maximum acceptable lower division (100-200 Level) credits transfered from another institution.</param>
        /// <param name="ui_minCreditsUpper">Minimum Credits from upper division (300+ Level).</param>
        /// <param name="ui_minCreditsResidency">Minimum credits taken at CWU.</param>
        /// <param name="ui_maxCreditsChallenge">Maximum credits from course challenges.</param>
        /// <param name="ui_minCreditsEarnedForMajor">Minimum credits taken in the major/minor field by transfer students.</param>
        /// <param name="ui_maxCreditsCoOp290">Naximum coop credits allowed at the 290 level.</param>
        /// <param name="ui_maxCreditsCoOp">Naximum coop credits allowed.</param>
        /// <param name="ui_maxCreditsCoOpTransfer">Naximum coop credits allowed for transfer students.</param>
        /// <param name="ui_maxCreditsCoOpGrad">Naximum coop credits allowed for graduate programs.</param>
        public CatalogCreditRequirements(uint ui_minCredits, uint ui_maxCreditsTransfer, uint ui_maxCreditsTransferLower, uint ui_minCreditsUpper, 
            uint ui_minCreditsResidency, uint ui_maxCreditsChallenge, uint ui_minCreditsEarnedForMajor, uint ui_maxCreditsCoOp290, uint ui_maxCreditsCoOp,
            uint ui_maxCreditsCoOpTransfer, uint ui_maxCreditsCoOpGrad)
        {
            this.ui_minCredits = ui_minCredits;
            this.ui_maxCreditsTransfer = ui_maxCreditsTransfer;
            this.ui_maxCreditsTransferLower = ui_maxCreditsTransferLower;
            this.ui_minCreditsUpper = ui_minCreditsUpper;
            this.ui_minCreditsResidency = ui_minCreditsResidency;
            this.ui_maxCreditsChallenge = ui_maxCreditsChallenge;
            this.ui_minCreditsEarnedForMajor = ui_minCreditsEarnedForMajor;
            this.ui_maxCreditsCoOp290 = ui_maxCreditsCoOp290;
            this.ui_maxCreditsCoOp = ui_maxCreditsCoOp;
            this.ui_maxCreditsCoOpTransfer = ui_maxCreditsCoOpTransfer;
            this.ui_maxCreditsCoOpGrad = ui_maxCreditsCoOpGrad;
        }

        /// <summary>Copy Constructor.</summary>
        /// <param name="other">Object to be copied.</param>
        public CatalogCreditRequirements(CatalogCreditRequirements other)
        {
            ui_minCredits = other.ui_minCredits;
            ui_maxCreditsTransfer = other.ui_maxCreditsTransfer;
            ui_maxCreditsTransferLower = other.ui_maxCreditsTransferLower;
            ui_minCreditsUpper = other.ui_minCreditsUpper;
            ui_minCreditsResidency = other.ui_minCreditsResidency;
            ui_maxCreditsChallenge = other.ui_maxCreditsChallenge;
            ui_minCreditsEarnedForMajor = other.ui_minCreditsEarnedForMajor;
            ui_maxCreditsCoOp290 = other.ui_maxCreditsCoOp290;
            ui_maxCreditsCoOp = other.ui_maxCreditsCoOp;
            ui_maxCreditsCoOpTransfer = other.ui_maxCreditsCoOpTransfer;
            ui_maxCreditsCoOpGrad = other.ui_maxCreditsCoOpGrad;
        }
    } // end structure CatalogCreditRequirements

    /// <summary>Structure storing degree-specific requirements.</summary>
    public struct DegreeRequirements : IComparable
    {
        private List<Course> l_generalRequirements;
        private List<Course> l_preAdmissionRequirements;
        private List<Course> l_coreRequirements;
        private List<Course> l_acceptableElectives;

        private uint ui_minElectiveCredits;
        private double ui_minMajorGPA;

        private string s_name;

        /// <summary>Constructor for this structure.</summary>
        /// <param name="l_generalRequirements">General University Requirements for this degree.</param>
        /// <param name="l_preAdmissionRequirements">Pre-admission requirements to enter the major.</param>
        /// <param name="l_coreRequirements">Core-course requirements to graduate.</param>
        /// <param name="l_acceptableElectives">List of acceptable electives for the degree.</param>
        /// <param name="ui_minElectiveCredits">Minimum number of electives credits for this degree.</param>
        /// <param name="ui_minMajorGPA">Minimum GPA required for this degree as major.</param>
        /// <param name="s_name">Name of this degree.</param>
        public DegreeRequirements(List<Course> l_generalRequirements, List<Course> l_preAdmissionRequirements,
                                  List<Course> l_coreRequirements, List<Course> l_acceptableElectives,
                                  uint ui_minElectiveCredits, double ui_minMajorGPA, string s_name)
        {
            this.l_generalRequirements = new List<Course>(l_generalRequirements);
            this.l_preAdmissionRequirements = new List<Course>(l_preAdmissionRequirements);
            this.l_coreRequirements = new List<Course>(l_coreRequirements);
            this.l_acceptableElectives = new List<Course>(l_acceptableElectives);
            this.ui_minElectiveCredits = ui_minElectiveCredits;
            this.ui_minMajorGPA = ui_minMajorGPA;
            this.s_name = s_name;
        } // end Constructor

        /// <summary>Copy Constructor.</summary>
        /// <param name="other">Object being copied.</param>
        public DegreeRequirements(DegreeRequirements other)
        {
            this.l_generalRequirements = new List<Course>(other.l_generalRequirements);
            this.l_preAdmissionRequirements = new List<Course>(other.l_preAdmissionRequirements);
            this.l_coreRequirements = new List<Course>(other.l_coreRequirements);
            this.l_acceptableElectives = new List<Course>(other.l_acceptableElectives);
            this.ui_minElectiveCredits = other.ui_minElectiveCredits;
            this.ui_minMajorGPA = other.ui_minMajorGPA;
            this.s_name = other.s_name;
        } // end Copy Constructor

        /// <summary>Compares the names of two degrees.</summary>
        /// <param name="obj">Other object to compare to.</param>
        /// <returns>String comparison between names.</returns>
        int IComparable.CompareTo(object obj)
        {
            DegreeRequirements d = (DegreeRequirements)obj;
            return String.Compare(s_name, d.s_name);
        }
    } // end structure DegreeRequirements

    /// <summary>Structure storing academic standing of a student.</summary>
    public struct AcademicStanding
    {
        /// <summary>Stores whether or not this student is a senior.</summary>
        private bool b_isSenior;
        /// <summary>Stores whether or not this student is in the CS major.</summary>
        private bool b_inMajor;
        /// <summary>Stores whether or not this student has good academic standing.</summary>
        private bool b_hasGoodStanding;

        /// <summary>Constructor for this structure.</summary>
        /// <param name="b_isSenior">Whether the student is a senior.</param>
        /// <param name="b_inMajor">Whether the student is in the respective major.</param>
        /// <param name="b_hasGoodStanding">Whether the student has good academic standing.</param>
        public AcademicStanding(bool b_isSenior, bool b_inMajor, bool b_hasGoodStanding)
        {
            this.b_isSenior = b_isSenior;
            this.b_inMajor = b_inMajor;
            this.b_hasGoodStanding = b_hasGoodStanding;
        }

        /// <summary>Copy Constructor.</summary>
        /// <param name="other">Object being copied.</param>
        public AcademicStanding(AcademicStanding other)
        {
            b_isSenior = other.b_isSenior;
            b_inMajor = other.b_inMajor;
            b_hasGoodStanding = other.b_hasGoodStanding;
        }

        /// <summary>Getter/Setter for the status of whether this student is in the CS major.</summary>
        public bool IsInMajor
        {
            get => b_inMajor;
            set => b_inMajor = value;
        } // end Major

        /// <summary>Getter/Setter for the status of whether this student is a senior.</summary>
        public bool IsSenior
        {
            get => b_isSenior;
            set => b_isSenior = value;
        } // end Senior

        /// <summary>Getter/Setter for the status of whether this student has good academic standing.</summary>
        public bool Standing
        {
            get => b_hasGoodStanding;
            set => b_hasGoodStanding = value;
        } // end Senior

    } // end structure AcademicStanding
} // end Namespace Database_Object_Classes
