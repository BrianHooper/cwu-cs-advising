namespace Database_Object_Classes
{
    /// <summary>Class storing the first and last name, student ID, catalog year, expected graduation quarter, and starting quarter of a student.</summary>
    public class Student : Database_Object
    {
        /// <summary>The name of this student.</summary>
        private Name    n_name;

        /// <summary>The quarter this student is expected to graduate.</summary>
        private Quarter q_expectedGraduation;
        /// <summary>The catalog year of this student.</summary>
        private Quarter q_startingQuarter;

        /// <summary>The number of credits this student has completed to date.</summary>
        private uint    ui_creditsCompleted;

        /// <summary>Stores whether or not this student is a senior.</summary>
        private bool    b_isSenior;
        /// <summary>Stores whether or not this student is in the CS major.</summary>
        private bool    b_inMajor;

        /// <summary>Constructor which creates a new student object with the specified name, ID, and starting quarter.</summary>
        /// <param name="n_name">The name of this student.</param>
        /// <param name="s_ID">Student ID of this student.</param>
        /// <param name="q_start">Quarter in which this student enrolled.</param>
        public Student(Name n_name, string s_ID, Quarter q_start) : base(s_ID)
        {
            this.n_name = new Name(n_name);
            q_startingQuarter = new Quarter(q_start);

            // default other variables
            q_expectedGraduation = Quarter.DefaultQuarter;
            b_inMajor = false;
            b_isSenior = false;
        } // end Default Constructor

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
   
        /// <summary>Getter/Setter for the number of credits this student has completed.</summary>
        public uint CreditsCompleted
        {
            get => ui_creditsCompleted;
            set => ui_creditsCompleted = value;
        } // end CreditsCompleted

    } // end Class Student
} // end Namespace Database_Object_Classes
