﻿using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;

namespace Database_Object_Classes 
{
    /// <summary>Enum for the season when a quarter occurs (Winter, Spring, etc.).</summary>
    /// <remarks>Winter = 0, Spring = 1, Summer = 2, Fall = 3.</remarks>
    [Serializable]
    public enum Season 
    {
        /// <summary>Winter quarter, denoted by 0.</summary>
        Winter,
        /// <summary>Spring quarter, denoted by 1.</summary>
        Spring,
        /// <summary>Summer quarter, denoted by 2.</summary>
        Summer,
        /// <summary>Fall quarter, denoted by 3.</summary>
        Fall,
        /// <summary>Represents an invalid state for this season object.</summary>
        Invalid
    } // end Enum Season

    /// <summary>Structure which contains the year, and "season" of the quarter.</summary>
    [Serializable]
    public struct Quarter
    {
        // Structure fields:
        /// <summary>The year of this Quarter.</summary>
        private uint ui_year;

        /// <summary>The season of this Quarter.</summary>
        private Season s_quarter;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
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
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Returns a default object of this structure.</summary>
        /// <returns>A default object of this structure.</returns>
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

        /// <summary>Postfix increment operator</summary>
        /// <param name="q">Operand</param>
        /// <returns>The next quarter.</returns>
        public static Quarter operator ++(Quarter q)
        {
            if (q.s_quarter == Season.Fall)
            {
                q.s_quarter = Season.Winter;
                q.ui_year++;
            } // end if
            else
            {
                q.s_quarter++;
            } // end else
            return q;
        } // end operator++

        /// <summary>Equality operator.</summary>
        /// <param name="lhs">Left-hand side operand.</param>
        /// <param name="rhs">Right-hand side operand.</param>
        /// <returns>True iff lhs is the same as rhs, otherwise false.</returns>
        public static bool operator==(Quarter lhs, Quarter rhs)
        {
            return lhs.s_quarter == rhs.s_quarter && lhs.ui_year == rhs.ui_year;
        } // end operator ==

        /// <summary>Inequality operator.</summary>
        /// <param name="lhs">Left-hand side operand.</param>
        /// <param name="rhs">Right-hand side operand.</param>
        /// <returns>True iff lhs is not the same as rhs, otherwise false.</returns>
        public static bool operator !=(Quarter lhs, Quarter rhs)
        {
            return !(lhs == rhs);
        } // end operator !=

        /// <summary>Checks if this object represents the same quarter as the given object.</summary>
        /// <param name="obj">Quarter to compare to.</param>
        /// <returns>True if the objects represent the same quarter, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            Quarter rhs = (Quarter)obj;
            return s_quarter == rhs.s_quarter && ui_year == rhs.ui_year;
        } // end operator equals

        /// <summary>Hashes this object's data.</summary>
        /// <returns>A basic hash code representing this object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        } // end method GetHashCode

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>For outputting Quarters to console.</summary>
        /// <returns>A string representation of this quarter.</returns>
        public override string ToString()
        {
            string name = "";
            switch(s_quarter)
            {
                case (Season)0:
                    name = "Winter";
                    break;
                case (Season)1:
                    name = "Spring";
                    break;
                case (Season)2:
                    name = "Summer";
                    break;
                case (Season)3:
                    name = "Fall";
                    break;
            } // end switch
            return name + " " + ui_year;
        } // end method ToString
    } // end structure Quarter

    /// <summary>Structure storing first and last name of a person.</summary>
    [Serializable]
    public struct Name
    {
        // Structure fields:
        /// <summary>The first name of this Student.</summary>
        private string s_fName;
        /// <summary>The first name of this Student.</summary>
        private string s_lName;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor for this structure.</summary>
        /// <param name="s_fname">First name.</param>
        /// <param name="s_lname">Last name.</param>
        public Name(string s_fname, string s_lname)
        {
            s_fName = string.Copy(s_fname);
            s_lName = string.Copy(s_lname);
        } // end Constructor
        
        /// <summary>Copy Constructor for this structure.</summary>
        /// <param name="n_other">The Name to be copied.</param>
        public Name(Name n_other)
        {
            s_fName = string.Copy(n_other.FirstName);
            s_lName = string.Copy(n_other.LastName);
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for the first name of this person.</summary>
        public string FirstName
        {
            get => s_fName;
            set => s_fName = string.Copy(value);
        } // end FirstName

        /// <summary>Getter/Setter for the last name of this person.</summary>
        public string LastName
        {
            get => s_lName;
            set => s_lName = string.Copy(value);
        } // end LastName

        /// <summary>Returns a default object of this structure.</summary>
        /// <returns>A default object of this structure.</returns>
        public static Name DefaultName => new Name("", "");

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>Returns this name as a string.</summary>
        /// <returns>The name in the format FirstName LastName.</returns>
        public override string ToString()
        {
            return s_fName + " " +  s_lName;
        } // end method ToString
    } // end structure Name

    /// <summary>Structure storing degree-specific requirements.</summary>
    [Serializable]
    public struct DegreeRequirements : IComparable
    {
        // Structure fields:
        /// <summary>General University Requirements for this degree.</summary>
        private List<Course> l_generalRequirements;

        private List<Course> l_acceptableElectives;

        private List<string> ls_generalRequirements;

        /// <summary>Minimum number of electives credits for this degree.</summary>
        private uint   ui_minElectiveCredits;

        /// <summary>Name of this degree.</summary>
        private string s_name;

        private string s_department;

        private string s_ID;

        private bool IsShallow;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor for this structure.</summary>
        /// <param name="s_name">Name of this degree.</param>
        /// <param name="l_generalRequirements">General University Requirements for this degree.</param>
        /// <param name="l_acceptableElectives">List of acceptable electives for the degree.</param>
        /// <param name="ui_minElectiveCredits">Minimum number of electives credits for this degree.</param>
        public DegreeRequirements(string s_name, List<Course> l_generalRequirements, List<Course> l_acceptableElectives,
                                  uint ui_minElectiveCredits)
        {
            this.l_generalRequirements      = new List<Course>(l_generalRequirements);
            this.l_acceptableElectives      = new List<Course>(l_acceptableElectives);
            this.ui_minElectiveCredits      = ui_minElectiveCredits;
            this.s_name                     = string.Copy(s_name);
            this.s_name                     = this.s_name.Replace(' ', '_');
            ls_generalRequirements          = new List<string>();
            IsShallow                       = false;
            s_ID                            = string.Empty;
            s_department                    = string.Empty;
        } // end Constructor

        /// <summary>Copy Constructor.</summary>
        /// <param name="other">Object being copied.</param>
        public DegreeRequirements(DegreeRequirements other)
        {
            l_generalRequirements       = new List<Course>(other.l_generalRequirements);
            l_acceptableElectives       = new List<Course>(other.l_acceptableElectives);
            ui_minElectiveCredits       = other.ui_minElectiveCredits;
            s_name                      = string.Copy(other.s_name);
            ls_generalRequirements      = new List<string>(other.ls_generalRequirements);
            IsShallow                   = other.IsShallow;
            s_ID                        = other.s_ID;
            s_department                = other.s_department;
        } // end Copy Constructor

        /// <summary>Shallow Constructor</summary>
        /// <param name="s_ID">Unique identifier of this degree.</param>
        /// <param name="s_name">Name of degree.</param>
        /// <param name="s_department">Department of this degree.</param>
        /// <param name="ls_generalRequirements">Shallow list of requirements.</param>
        public DegreeRequirements(string s_ID, string s_name, string s_department, List<string> ls_generalRequirements)
        {
            this.l_generalRequirements  = new List<Course>();
            this.l_acceptableElectives  = new List<Course>();
            this.ui_minElectiveCredits  = 0;
            this.s_name                 = string.Copy(s_name);
            this.ls_generalRequirements = new List<string>(ls_generalRequirements);
            IsShallow                   = true;
            this.s_ID                   = string.Copy(s_ID);
            this.s_department           = string.Copy(s_department);
        } // end Constructor

        /// <summary>Regular Constructor</summary>
        /// <param name="s_ID">Unique identifier of this degree.</param>
        /// <param name="s_name">Name of degree.</param>
        /// <param name="s_department">Department of this degree.</param>
        /// <param name="l_generalRequirements">List of requirements for this degree.</param>
        public DegreeRequirements(string s_ID, string s_name, string s_department, List<Course> l_generalRequirements)
        {
            this.l_generalRequirements  = new List<Course>(l_generalRequirements);
            this.l_acceptableElectives  = new List<Course>();
            this.ui_minElectiveCredits  = 0;
            this.s_name                 = string.Copy(s_name);
            this.ls_generalRequirements = new List<string>();

            foreach(Course c in l_generalRequirements)
            {
                ls_generalRequirements.Add(c.ID);
            } // end foreach

            IsShallow                   = false;
            this.s_ID                   = string.Copy(s_ID);
            this.s_department           = string.Copy(s_department);
        } // end Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for this Degree's name.</summary>
        public string Name
        {
            get
            {
                string temp = string.Copy(s_name);
                return temp.Replace('_',' ');
            } // end get
            set
            {
                s_name = string.Copy(value);
                s_name.Replace(' ', '_');
            } // end set
        } // end Name

        /// <summary>Getter/Setter for this Degree's department. </summary>
        public string Department
        {
            get => s_department;
            set => s_department = string.Copy(value);
        }

        /// <summary>Getter for the ID of this course, which is the name with all spaces replaced with underscores.</summary>
        /// <remarks>This should only be used for database operations.</remarks>
        public string ID => s_name;

        /// <summary>Getter/Setter for this Degree's minimum required elective credits.</summary>
        public uint MinimumElectiveCredits
        {
            get => ui_minElectiveCredits;
            set => ui_minElectiveCredits = value;
        } // end MinimumElectiveCredits

        /// <summary>Getter/Setter for this Degree's general course requirements.</summary>
        public ReadOnlyCollection<Course> Requirements
        {
            get => l_generalRequirements.AsReadOnly();
            set
            {
                l_generalRequirements = new List<Course>(value);
                ls_generalRequirements.Clear();
                foreach(Course c in value)
                {
                    ls_generalRequirements.Add(c.ID);
                } // end foreach
            } // end set
        } // end GeneralRequirements

        /// <summary>Getter/Setter for this Degree's general course requirements (shallow version).</summary>
        public List<String> ShallowRequirements
        {
            get => ls_generalRequirements;
            set => ls_generalRequirements = new List<string>(value);
        } // end ShallowRequirements

        /// <summary>Getter/Setter for this Degree's acceptable elective courses.</summary>
        public ReadOnlyCollection<Course> AcceptableElectives
        {
            get => l_acceptableElectives.AsReadOnly();
            set => l_acceptableElectives = new List<Course>(value);
        } // end AcceptableElectives

        /// <summary>Returns a default object of this structure.</summary>
        /// <returns>A default object of this structure.</returns>
        public static DegreeRequirements DefaultDegreeRequirements => new DegreeRequirements("", new List<Course>(), new List<Course>(), 0);

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>Compares the names of two degrees.</summary>
        /// <param name="obj">Other object to compare to.</param>
        /// <returns>String comparison between names.</returns>
        int IComparable.CompareTo(object obj)
        {
            DegreeRequirements d = (DegreeRequirements)obj;
            return string.Compare(s_name, d.s_name);
        } // end method CompareTo

		public override string ToString()
		{
            string str = "Degree: " + ID;

            str += "\nCourse Requirements: ";
            foreach (string s in ls_generalRequirements)
            {
                str += "\n " + s;
            } // end foreach

			return str;
		} // end method ToString
	} // end structure DegreeRequirements

    /// <summary>Structure storing academic standing of a student.</summary>
    [Serializable]
    public struct AcademicStanding
    {
        // Structure fields:
        /// <summary>Stores whether or not this student is a senior.</summary>
        private bool b_isSenior;
        /// <summary>Stores whether or not this student is in the CS major.</summary>
        private bool b_inMajor;
        /// <summary>Stores whether or not this student has good academic standing.</summary>
        private bool b_hasGoodStanding;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
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

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for the status of whether this student is in the CS major.</summary>
        public bool InMajor
        {
            get => b_inMajor;
            set => b_inMajor = value;
        } // end InMajor

        /// <summary>Getter/Setter for the status of whether this student is a senior.</summary>
        public bool Senior
        {
            get => b_isSenior;
            set => b_isSenior = value;
        } // end Senior

        /// <summary>Getter/Setter for the status of whether this student has good academic standing.</summary>
        public bool Standing
        {
            get => b_hasGoodStanding;
            set => b_hasGoodStanding = value;
        } // end Standing    

        /// <summary>Returns a default object of this structure.</summary>
        /// <returns>A default object of this structure.</returns>
        public static AcademicStanding DefaultAcademicStanding => new AcademicStanding(false, false, false);

        /// <summary>
        /// Testing tostring method.
        /// </summary>
        /// <returns>the objects data in string form.</returns>
        public override string ToString()
        {
            return "\n  Senior: " + b_isSenior.ToString() + "\n  InMajor: " + b_inMajor.ToString() + "\n  Good Standing: " + b_hasGoodStanding.ToString();            
        }
    } // end structure AcademicStanding

    /// <summary>Structure storing graduation plan information.</summary>
    [Serializable]
    public struct PlanInfo
    {
        // Structure fields:
        /// <summary>This Plan's owner's ID.</summary>
        private readonly string s_SID;

        /// <summary>The starting quarter of this plan.</summary>
        private Quarter         q_start;

        /// <summary>The classes in this plan.</summary>
        private string[]        sa_classes;

        /// <summary>The write protect value of this plan.</summary>
        private readonly uint   ui_WP;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor for this structure.</summary>
        /// <param name="s_ID">The ID of the owner of this plan.</param>
        /// <param name="ui_WP">Write Protect value on this plan.</param>
        /// <param name="s_qtr">The starting quarter in string form.</param>
        /// <param name="sa_classes">The classes in this plan.</param>
        /// <remarks>The quarter passed must be in the form "Season Year".</remarks>
        public PlanInfo(string s_ID, uint ui_WP, string s_qtr, string[] sa_classes)
        {
            s_SID = string.Copy(s_ID);

            this.ui_WP = ui_WP;

            this.sa_classes = new string[sa_classes.Length];
            Array.Copy(sa_classes, this.sa_classes, sa_classes.Length);

            // convert quarter info from string to quarter object
            string[] temp = s_qtr.Split();

            Season s;

            // convert string to season
            switch(temp[0])
            {
                case "Winter":
                    s = Season.Winter;
                    break;
                case "Spring":
                    s = Season.Spring;
                    break;
                case "Summer":
                    s = Season.Summer;
                    break;
                case "Fall":
                    s = Season.Fall;
                    break;
                default:
                    s = Season.Invalid;
                    break;
            } // end switch

            // convert string to uint
            uint.TryParse(temp[1], out uint yr);

            q_start = new Quarter(yr, s);
        } // end Constructor

        /// <summary>Copy Constructor for this structure.</summary>
        /// <param name="other">PlanInfo object to copy.</param>
        public PlanInfo(PlanInfo other)
        {
            s_SID   = string.Copy(other.StudentID);
            q_start = new Quarter(other.StartQuarter);
            ui_WP    = other.ui_WP;

            sa_classes = new string[other.sa_classes.Length];
            Array.Copy(other.sa_classes, sa_classes, sa_classes.Length);
        } // end Copy Constructor

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter/Setter for this Plan's starting quarter.</summary>
        public Quarter StartQuarter
        {
            get => q_start;
            set => q_start = new Quarter(value);
        } // end Quarter

        /// <summary>Getter/Setter for this Plan's classes.</summary>
        public string[] Classes
        {
            get => sa_classes;
            set
            {
                sa_classes = new string[value.Length];
                Array.Copy(sa_classes, value, sa_classes.Length);
            } // end set
        } // end classes

        /// <summary>Getter for this Plan's owner.</summary>
        public string StudentID => s_SID;

        /// <summary>Getter for the write protect value of this plan.</summary>
        public uint WP => ui_WP;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>Turns the data in this object into a string for output.</summary>
        /// <returns>A string containing the data in this object.</returns>
        public override string ToString()
        {
            string str = "Quarter: ";
            str += q_start.ToString();
            str += "\nClasses:";
            
            foreach (string s in sa_classes)
            {
                str += "\n";
                str += s;
            } // end foreach

            return str;
        } // end method ToString
    } // end structure PlanInfo

    /// <summary>Structure storing user information.</summary>
    [Serializable]
    public struct Credentials
    {
        // Structure fields:
        /// <summary>Username of the owner of these credentials.</summary>
        private readonly string s_userName;
        /// <summary>The password salt for this user. This should never be changed.</summary>
        private byte[] ba_PWSalt;

        /// <summary>Whether or not this user is an administrator.</summary>
        private bool b_isAdmin;

        /// <summary>Whether or not this user's account is currently active.</summary>
        private bool b_isActive;

        /// <summary>Write protect value of this object.</summary>
        private readonly uint ui_WP;

        /// <summary>Contains this user's password.</summary>
        /// <remarks>
        ///          Retrieve will not fill this variable.
        ///          This variable should only be set when the user's password is to be changed.
        /// </remarks>
        private readonly string s_pw;

        private byte[] ba_pw;

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Constructors:
        /// <summary>Constructor for this structure.</summary>
        /// <param name="s_ID">Username of the owner of these credentials.</param>
        /// <param name="ui_WP">Write protect value of this object.</param>
        /// <param name="b_isAdmin">Whether or not this user is an administrator.</param>
        /// <param name="b_isActive">Whether or not this user's account is active.</param>
        /// <param name="ba_PWSalt">The password salt for this user.</param>
        /// <param name="s_password">A hash of the user's password.</param>
        /// <remarks>The only thing that should be changed in this structure is the IsAdmin, IsActive, or Password value.
        ///          Changing the username will create a new user upon write back to the DB.
        ///          Changing the password salt will have no effect on the DB, 
        ///          as this value will be disregarded upon write back.
        ///          Changing the write protect will either cause an error, or data corruption.
        /// </remarks>
        public Credentials(string s_ID, uint ui_WP, bool b_isAdmin, bool b_isActive, byte[] ba_PWSalt, string s_password)
        {
            s_userName      = string.Copy(s_ID);
            this.ba_PWSalt  = new byte[ba_PWSalt.Length];
            this.ui_WP      = ui_WP;
            this.b_isAdmin  = b_isAdmin;
            this.b_isActive = b_isActive;
            s_pw            = string.Copy(s_password);
            ba_pw           = new byte[64];

            Array.Copy(ba_PWSalt, this.ba_PWSalt, this.ba_PWSalt.Length);
        } // end Constructor

        /// <summary>Copy Constructor for this structure.</summary>
        /// <param name="other">Credentials object to copy into this object.</param>
        /// <remarks>The password is not copied from the other object.</remarks>
        public Credentials(Credentials other)
        {
            s_userName  = other.UserName;
            ba_PWSalt   = other.ba_PWSalt;
            ui_WP       = other.WP;
            b_isAdmin   = other.IsAdmin;
            b_isActive  = other.IsActive;
            s_pw        = String.Empty;
            ba_pw       = new byte[64];
            Array.Copy(other.ba_pw, ba_pw, 64);
        } // end Copy Constructor              

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // General Getters/Setters:
        /// <summary>Getter for the username of the owner of these credentials.</summary>
        public string UserName => s_userName;

        /// <summary>Getter for the password salt of this user.</summary>
        public byte[] PWSalt
        {
            get
            {
                return ba_PWSalt;
            }
            set
            {
                ba_PWSalt = new byte[32];
                Array.Copy(value, ba_PWSalt, 32);
            }
        }

        /// <summary>Getter/Setter for the byte array containing the user's password hash.</summary>
        public byte[] Password_Hash
        {
            get
            {
                return ba_pw;
            } // end get
            set
            {
                ba_pw = new byte[value.Length];
                Array.Copy(value, ba_pw, ba_pw.Length);
            } // end set
        } // end Password_Hash

        /// <summary>Getter/Setter for whether this user is an Admin.</summary>
        public bool IsAdmin
        {
            get => b_isAdmin;
            set => b_isAdmin = value;
        } // end isAdmin

        /// <summary>Getter/Setter for whether this user's account is active.</summary>
        public bool IsActive
        {
            get => b_isActive;
            set => b_isActive = value;
        }

        /// <summary>Getter for the write protect value of these user credentials.</summary>
        public uint WP => ui_WP;

        /// <summary>Returns the string containing the password hash. </summary>
        public string Password => s_pw;
        

        /* * * * * * * * * * * * * * * * * * * * * * * * * */

        // Methods:
        /// <summary>Turns the data in this object into a string for output.</summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string str = "Username: ";
            str += s_userName;
            str += "\nWP: ";
            str += ui_WP.ToString();
            str += "\nAdmin: ";
            str += b_isAdmin.ToString();
            str += "\nSalt: ";
            str += "0x" + BitConverter.ToString(ba_PWSalt).Replace("-"," ");
            str += "\nUser Status: ";
            str += b_isActive ? "Active" : "Inactive";

            return str;
        }
    } // end structure Credentials
} // end Namespace Database_Object_Classes