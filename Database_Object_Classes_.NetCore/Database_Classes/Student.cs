using System;

namespace Database_Object_Classes
{
    /// <summary>Class storing the first and last name, student ID, catalog year, expected graduation quarter, and starting quarter of a student.</summary>
    [Serializable]
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
            
            q_expectedGraduation = Quarter.DefaultQuarter;
        } // end Constructor
        
        /// <summary>Copy constructor which copies the contents of one student into this student.</summary>
        /// <param name="other">The object to copy into this one.</param>
        public Student(Student other) : base(other)
        {
            n_name               = new Name(other.Name);
            q_startingQuarter    = new Quarter(other.q_startingQuarter);
            q_expectedGraduation = new Quarter(other.q_expectedGraduation);
            as_standing          = new AcademicStanding(other.as_standing);
            d_GPA                = other.d_GPA;
            ui_creditsCompleted  = other.ui_creditsCompleted;
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for Expected Graduation</summary>
        public Quarter ExpectedGraduation
        {
            get => q_expectedGraduation;
            set => q_expectedGraduation = new Quarter(value);
        } // end ExpectedGraduation

        /// <summary>Getter/Setter for the number of credits this student has completed.</summary>
        public uint CreditsCompleted
        {
            get => ui_creditsCompleted;
            set => ui_creditsCompleted = value;
        } // end CreditsCompleted

        /// <summary>Getter/Setter for student name.</summary>
        public Name Name
        {
            get => n_name;
            set => n_name = new Name(value);
        } // end Name

        /// <summary>Getter/Setter for student GPA.</summary>
        public double GPA
        {
            get => d_GPA;
            set => d_GPA = value;
        } // end GPA

        /// <summary>Getter/Setter for student starting quarter.</summary>
        public Quarter StartingQuarter
        {
            get => q_startingQuarter;
            set => q_startingQuarter = new Quarter(value);
        } // end StartingQuarter

        /// <summary>Getter/Setter for student Academic Standing.</summary>
        public AcademicStanding AcademicStanding
        {
            get => as_standing;
            set => as_standing = new AcademicStanding(as_standing);
        } // end AcademicStanding
    } // end Class Student
} // end Namespace Database_Object_Classes