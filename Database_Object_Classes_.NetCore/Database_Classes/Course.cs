using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Database_Object_Classes
{
    /// <summary>Class storing the name, ID, and prerequisits for a course.</summary>
    public class Course : Database_Object 
    {
        // Class fields:
        /// <summary>Number of quarters per year constant.</summary>
        private const int     i_NUMBERQUARTERS = 4;

        /// <summary>The number of credits this course is worth.</summary>
        private uint          ui_numberCredits;

        /// <summary>Name of this course.</summary>
        private string        s_name;

        /// <summary>Prerequisites for this course.</summary>
        private List<Course>  l_preRequisites;

        /// <summary>The quarters this course is offered, [0] = Winter, [3] = Fall.</summary>
        private bool[]        ba_quartersOffered;

        /// <summary>Stores whether or not this quarter requires a student to be in the CS major to take it.</summary>
        private bool          b_requiresMajor;


        // Constructors:
        /// <summary>Default Constructor.</summary>
        public Course() : base("")
        {
            this.s_name = "";
            this.ui_numberCredits = 0;
            this.b_requiresMajor = false;

            ba_quartersOffered = new bool[i_NUMBERQUARTERS];

            l_preRequisites = new List<Course>();
        } // end default Constructor

        /// <summary>Constructor which creates a Course object with a name and ID, and sets all other fields to default.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="b_requiresMajor">The status of this course requiring a student to be in the CS major.</param>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.</remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool b_requiresMajor) : base(s_ID)
        {
            this.s_name = s_name;
            this.ui_numberCredits = ui_numberCredits;
            this.b_requiresMajor = b_requiresMajor;

            ba_quartersOffered = new bool[i_NUMBERQUARTERS];

            l_preRequisites = new List<Course>();
        } // end Constructor

        /// <summary>Constructor which creates a Course object with a name, ID, and the given prerequisites.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="b_requiresMajor">The status of this course requiring a student to be in the CS major.</param>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The course pre-requisites can be in any collection that implements the <see cref="System.Collections.Generic.ICollection{T}"/> interface.</remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool b_requiresMajor, ICollection<Course> col_courses) 
            : this(s_name, s_ID, ui_numberCredits, b_requiresMajor) => AddPreRequisite(col_courses);

        /// <summary>Constructor which creates a Course object with a name, ID, and the given quarters it's offered.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="b_requiresMajor">The status of this course requiring a student to be in the CS major.</param>
        /// <param name="ba_quarters">The quarters when the class is offered.</param>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The quarters array considers [0] to be winter, and [3] to be fall. 
        /// </remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool b_requiresMajor, bool[] ba_quarters) 
            : this(s_name, s_ID, ui_numberCredits, b_requiresMajor) => SetQuarterOffered(ba_quarters);

        /// <summary>Constructor which creates a Course object with a name, an ID, the given quarters it's offered, and the given prerequisites.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="b_requiresMajor">The status of this course requiring a student to be in the CS major.</param>
        /// <param name="ba_quarters">The quarters when the class is offered.</param>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The quarters array considers [0] to be winter, and [3] to be fall.
        ///          The course prerequisites can be in any collection that implements the <see cref="System.Collections.Generic.ICollection{T}"/> interface.
        /// </remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool b_requiresMajor, bool[] ba_quarters, ICollection<Course> col_courses) 
            : this(s_name, s_ID, ui_numberCredits, b_requiresMajor, ba_quarters) => AddPreRequisite(col_courses);


        // General Getters/Setters:
        /// <summary>Getter/Setter for name of this course.</summary>
        public string Name
        {
            get => s_name;
            set
            {
                s_name = value;

                ObjectAltered();
            } // end set
        } // end Name

        /// <summary>Getter for PreRequisites list of this course.</summary>
        /// <remarks>The returned collection will be a read-only collection, and may not be modified directly.</remarks>
        public ReadOnlyCollection<Course> PreRequisites => (l_preRequisites.AsReadOnly());

        /// <summary>Getter for quarters offered array of this course.</summary>
        public bool[] QuartersOffered => ba_quartersOffered;

        /// <summary>Getter/Setter for whether this course requires a student to be a CS major.</summary>
        public bool RequiresMajor
        {
            get => b_requiresMajor;
            set
            {
                b_requiresMajor = value;

                ObjectAltered();
            } // end set
        } // end RequiresMajor


        // QuartersOffered getters/setters:
        /// <summary>Checks whether this course is offered in the specified quarter.</summary>
        /// <param name="se_i">Quarter to check.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.</remarks>
        /// <returns>True if offered, false if not offered.</returns>
        public bool IsOffered(Season se_i) => ba_quartersOffered[(int)se_i];

        /// <summary>Sets a specific quarter to the given status.</summary>
        /// <param name="se_i">Quarter to set.</param>
        /// <param name="b_status">The new offering-status for this course in the specified quarter.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.
        ///          True = offered, False = not offered
        ///          This will change only a single quarter, to change all use <see cref="SetQuarterOffered(bool[])"/>.
        /// </remarks>
        public void SetQuarterOffered(Season se_i, bool b_status)
        {
            ba_quartersOffered[(int)se_i] = b_status;

            ObjectAltered();
        } // end method setQuarterOffered

        /// <summary>Setter for quarters offered array of this course.</summary>
        /// <param name="ba_quarters">Array containing the new offering-status of this course for all quarters.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.
        ///          True = offered, False = not offered
        ///          All previous offerings will be overriden by this method. To change only a single quarter use <see cref="SetQuarterOffered(Season, bool)"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        public void SetQuarterOffered(bool[] ba_quarters)
        {
            if (ba_quarters.Length != i_NUMBERQUARTERS)
            {
                throw new System.ArgumentException("Invalid Array size of ba_quarters passed to setQuarterOffered. Expected: 4; Actual: " + ba_quarters.Length);
            } // end if

            for (int i = 0; i < i_NUMBERQUARTERS; i++)
            {
                ba_quartersOffered[i] = ba_quarters[i];
            } // end for

            ObjectAltered();
        } // end method setQuarterOffered


        // Prerequisite setters:
        /// <summary>Adds the given Course to the prerequisites list of this Course object.</summary>
        /// <param name="c_course">A course prerequisites for this course to be added to the prerequisites list.</param>
        public void AddPreRequisite(Course c_course)
        {
            if (!l_preRequisites.Contains(c_course))
            {
                l_preRequisites.Add(c_course);

                ObjectAltered();
            }
        } // end method addPreRequisite

        /// <summary>Adds the given Courses to the prerequisites list of this Course object.</summary>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        public void AddPreRequisite(ICollection<Course> col_courses)
        {
            foreach (Course c_course in col_courses)
            {
                if (!l_preRequisites.Contains(c_course))
                {
                    l_preRequisites.Add(c_course);
                }
            } // end foreach

            ObjectAltered();
        } // end method addPreRequisite

        /// <summary>Removes the course whith the specified ID from the prerequisites of this Course object, given it exists.</summary>
        /// <param name="s_courseID">The ID of the course to be removed from to the prerequisites list.</param>
        /// <returns>True if the course was found, and successfully removed. False if it could not be removed.</returns>
        public bool RemovePreRequisite(string s_courseID)
        {
            foreach (Course course in l_preRequisites)
            {
                if (course.ID == s_courseID)
                {
                    ObjectAltered();

                    return l_preRequisites.Remove(course);
                } // end if
            } // end foreach

            return false; // not found
        } // end method removePreRequisite

        /// <summary>Removes all prerequisites of this Course object.</summary>
        public void ClearPreRequisites()
        {
            l_preRequisites.Clear();

            ObjectAltered();
        } // end method clearPreRequisites

    } // end Class Course 
} // end Namespace Database_Object_Classes
