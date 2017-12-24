namespace Database_Object_Classes
{
    /// <summary>Class storing the first and last name, student ID, catalog year, expected graduation quarter, and starting quarter of a student.</summary>
    public class Student : Database_Object
    {
        // Class fields:
        /// <summary>The number of credits this student has completed to date.</summary>
        private uint             ui_creditsCompleted;

        /// <summary>Stores this student's GPA.</summary>
        private double           d_GPA;

        /// <summary>The name of this student.</summary>
        private Name             n_name;

        /// <summary>The catalog year of this student.</summary>
        private Quarter          q_startingQuarter;
        /// <summary>The quarter this student is expected to graduate.</summary>
        private Quarter          q_expectedGraduation;

        /// <summary>The academic standing of this student.</summary>
        private AcademicStanding as_standing;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor which creates a new student object with the specified name, ID, and starting quarter.</summary>
        /// <param name="n_name">The name of this student.</param>
        /// <param name="s_ID">Student ID of this student.</param>
        /// <param name="q_startingQuarter">Quarter in which this student enrolled.</param>
        public Student(Name n_name, string s_ID, Quarter q_startingQuarter) : base(s_ID)
        {
            this.n_name            = new Name(n_name);
            this.q_startingQuarter = new Quarter(q_startingQuarter);

            // default other variables
            q_expectedGraduation  = Quarter.DefaultQuarter;
            ui_creditsCompleted   = 0;
            as_standing           = AcademicStanding.DefaultAcademicStanding;
            d_GPA                 = 0;
        } // end Constructor

        /// <summary>Constructor for all fields.</summary>
        /// <param name="n_name">This student's name.</param>
        /// <param name="s_ID">This student's SID.</param>
        /// <param name="q_startingQuarter">The quarter in which this student started.</param>
        /// <param name="ui_creditsCompleted">The number of credits this student has completed.</param>
        /// <param name="d_GPA">This student's GPA.</param>
        /// <param name="as_standing">This student's academic standing.</param>
        public Student(Name n_name, string s_ID, Quarter q_startingQuarter, uint ui_creditsCompleted, double d_GPA, AcademicStanding as_standing) : base(s_ID)
        {
            this.n_name              = new Name(n_name);
            this.q_startingQuarter   = new Quarter(q_startingQuarter);            
            this.as_standing         = new AcademicStanding(as_standing);
            this.ui_creditsCompleted = ui_creditsCompleted;
            this.d_GPA               = d_GPA;

            // expected graduation is still set to default
            q_expectedGraduation = Quarter.DefaultQuarter;
        } // end Constructor
        
        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for Expected Graduation</summary>
        public Quarter ExpectedGraduation
        {
            get => q_expectedGraduation;
            set
            {
                ObjectAltered();
                q_expectedGraduation = new Quarter(value);                
            } // end set
        } // end ExpectedGraduation

        /// <summary>Getter/Setter for the number of credits this student has completed.</summary>
        public uint CreditsCompleted
        {
            get => ui_creditsCompleted;
            set
            {
                ObjectAltered();
                ui_creditsCompleted = value;
            } // end set
        } // end CreditsCompleted

        /// <summary>Getter/Setter for student name.</summary>
        public Name Name
        {
            get => n_name;
            set
            {
                ObjectAltered();
                n_name = new Name(value);
            } // end set
        } // end Name
    } // end Class Student
} // end Namespace Database_Object_Classes