namespace Database_Object_Classes
{
    /// <summary>The season when a quarter occurs (Winter, Spring, etc.).</summary>
    /// <remarks>Winter = 0, Spring = 1, Summer = 2, Fall = 3.</remarks>
    public enum Season
    {
        Winter, Spring, Summer, Fall
    }

    /// <summary>Structure which contains the year, and "season" of the quarter.</summary>
    public struct Quarter
    {
        private uint ui_year;
        private Season s_quarter;

        public uint Year
        {
            get
            {
                return ui_year;
            } // end get 
            set
            {
                ui_year = value;
            } // end set 
        } // end Year

        public Season QuarterSeason
        {
            get
            {
                return s_quarter;
            } // end get 
            set
            {
                s_quarter = value;
            } // end set 
        } // end StartQuarter

        public Quarter(uint ui_yr, Season s_qtr)
        {
            ui_year = ui_yr;
            s_quarter = s_qtr;
        } // end Constructor
    } // end structure Quarter
} // end Namespace Database_Object_Classes
