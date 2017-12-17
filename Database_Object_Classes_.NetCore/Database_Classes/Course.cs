using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Database_Object_Classes
{
    /// <summary>Class storing the name, ID, and prerequisits for a given course.</summary>
    public class Course : Database_Object 
    {
        // Class fields:
        /// <summary>Number of quarters per year constant.</summary>
        private const int    i_NUMBERQUARTERS = 4;

        /// <summary>The number of credits this course is worth.</summary>
        private readonly uint ui_numberCredits;

        /// <summary>Name of this course.</summary>
        private string       s_name;

        /// <summary>Prerequisites for this course.</summary>
        private List<Course> l_preRequisites;

        /// <summary>The quarters this course is offered, [0] = Winter, [3] = Fall.</summary>
        private bool[]       ba_quartersOffered;


        // Constructors:
        /// <summary>Default Constructor. Creates a Course object with a name and ID, and sets all other fields to default.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.</remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits) : base(s_ID)
        {
            this.s_name = s_name;
            this.ui_numberCredits = ui_numberCredits;

            ba_quartersOffered = new bool[i_NUMBERQUARTERS];

            l_preRequisites = new List<Course>();
        } // end Default Constructor

        /// <summary>Constructor which creates a Course object with a name, ID, and the given prerequisites.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The course pre-requisites can be in any collection that implements the <see cref="System.Collections.Generic.ICollection{T}"/> interface.</remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, ICollection<Course> col_courses) : this(s_name, s_ID, ui_numberCredits)
        {
            addPreRequisite(col_courses);
        } // end Constructor

        /// <summary>Constructor which creates a Course object with a name, ID, and the given quarters it's offered.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ba_quarters">The quarters when the class is offered.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The quarters array considers [0] to be winter, and [3] to be fall. 
        /// </remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool[] ba_quarters) : this(s_name, s_ID, ui_numberCredits)
        {
            setQuarterOffered(ba_quarters);
        } // end Constructor

        /// <summary>Constructor which creates a Course object with a name, an ID, the given quarters it's offered, and the given prerequisites.</summary>
        /// <param name="s_name">The course name.</param>
        /// <param name="s_ID">The course identifier.</param>
        /// <param name="ui_numberCredits">The number of credits this course is worth.</param>
        /// <param name="ba_quarters">The quarters when the class is offered.</param>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        /// <remarks>The course name is the actual name of the course, e.g. Computer Architecture 1.
        ///          The course identifier is the unique identifier for this course, e.g. CS311 which must not contain spaces.
        ///          The quarters array considers [0] to be winter, and [3] to be fall.
        ///          The course prerequisites can be in any collection that implements the <see cref="System.Collections.Generic.ICollection{T}"/> interface.
        /// </remarks>
        public Course(string s_name, string s_ID, uint ui_numberCredits, bool[] ba_quarters, ICollection<Course> col_courses) : this(s_name, s_ID, ui_numberCredits, ba_quarters)
        {
            addPreRequisite(col_courses);
        } // end Constructor


        // General Getters/Setters:
        /// <summary>Getter/Setter for name of this course.</summary>
        public string Name
        {
            get
            {
                return s_name;
            } // end get
            set
            {
                s_name = value;

                objectAltered();
            } // end set
        } // end Name

        /// <summary>Getter for PreRequisites list of this course.</summary>
        /// <remarks>The returned collection will be a read-only collection, and may not be modified directly.</remarks>
        public ReadOnlyCollection<Course> PreRequisites
        {
            get
            {
                return (l_preRequisites.AsReadOnly());
            } // end get
        } // end PreRequisites

        /// <summary>Getter for quarters offered array of this course.</summary>
        public bool[] QuartersOffered
        {
            get
            {
                return ba_quartersOffered;
            } // end get
        } // end QuartersOffered


        // QuartersOffered getters/setters:
        /// <summary>Checks whether this course is offered in the specified quarter.</summary>
        /// <param name="se_i">Quarter to check.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.</remarks>
        /// <returns>True if offered, false if not offered.</returns>
        public bool isOffered(Season se_i)
        {
            return ba_quartersOffered[(int)se_i];
        } // end method isOffered

        /// <summary>Sets a specific quarter to the given status.</summary>
        /// <param name="se_i">Quarter to set.</param>
        /// <param name="b_status">The new offering-status for this course in the specified quarter.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.
        ///          True = offered, False = not offered
        ///          This will change only a single quarter, to change all use <see cref="setQuarterOffered(bool[])"/>.
        /// </remarks>
        public void setQuarterOffered(Season se_i, bool b_status)
        {
            ba_quartersOffered[(int)se_i] = b_status;

            objectAltered();
        } // end method setQuarterOffered

        /// <summary>Setter for quarters offered array of this course.</summary>
        /// <param name="ba_quarters">Array containing the new offering-status of this course for all quarters.</param>
        /// <remarks>Quarter 0 is Winter, quarter 3 is fall.
        ///          True = offered, False = not offered
        ///          All previous offerings will be overriden by this method. To change only a single quarter use <see cref="setQuarterOffered(Season, bool)"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentException">This exception is thrown if the ba_quarters array is not size 4.</exception>
        public void setQuarterOffered(bool[] ba_quarters)
        {
            if (ba_quarters.Length != i_NUMBERQUARTERS)
            {
                throw new System.ArgumentException("Invalid Array size of ba_quarters passed to setQuarterOffered. Expected: 4; Actual: " + ba_quarters.Length);
            } // end if

            for (int i = 0; i < i_NUMBERQUARTERS; i++)
            {
                ba_quartersOffered[i] = ba_quarters[i];
            } // end for

            objectAltered();
        } // end method setQuarterOffered


        // Prerequisite setters:
        /// <summary>Adds the given Course to the prerequisites list of this Course object.</summary>
        /// <param name="c_course">A course prerequisites for this course to be added to the prerequisites list.</param>
        public void addPreRequisite(Course c_course)
        {
            if (!l_preRequisites.Contains(c_course))
            {
                l_preRequisites.Add(c_course);

                objectAltered();
            }
        } // end method addPreRequisite

        /// <summary>Adds the given Courses to the prerequisites list of this Course object.</summary>
        /// <param name="col_courses">A collection of prerequisites for this course to be added to the prerequisites list.</param>
        public void addPreRequisite(ICollection<Course> col_courses)
        {
            foreach (Course c_course in col_courses)
            {
                if (!l_preRequisites.Contains(c_course))
                {
                    l_preRequisites.Add(c_course);
                }
            } // end foreach

            objectAltered();
        } // end method addPreRequisite

        /// <summary>Removes the course whith the specified ID from the prerequisites of this Course object, given it exists.</summary>
        /// <param name="s_courseID">The ID of the course to be removed from to the prerequisites list.</param>
        /// <returns>True if the course was found, and successfully removed. False if it could not be removed.</returns>
        public bool removePreRequisite(string s_courseID)
        {
            foreach (Course course in l_preRequisites)
            {
                if (course.ID == s_courseID)
                {
                    objectAltered();

                    return l_preRequisites.Remove(course);
                } // end if
            } // end foreach

            return false; // not found
        } // end method removePreRequisite

        /// <summary>Removes all prerequisites of this Course object.</summary>
        public void clearPreRequisites()
        {
            l_preRequisites.Clear();

            objectAltered();
        } // end method clearPreRequisites

    } // end Class Course 
} // end Namespace Database_Object_Classes
