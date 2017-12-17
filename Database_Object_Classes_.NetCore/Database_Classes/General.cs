namespace Database_Object_Classes
{
    /// <summary>The season when a quarter occurs (Winter, Spring, etc.).</summary>
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

    }
} // end Namespace Database_Object_Classes
