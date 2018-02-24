using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CwuAdvising.Models;
using Database_Object_Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PlanGenerationAlgorithm;
using static CwuAdvising.Models.ScheduleModel;

namespace CwuAdvising.Pages
{
    /// <summary>Model for student advising</summary>
    public class AdvisingModel : PageModel
    {
        /// <summary>Current student loaded to advising page</summary>
        public static StudentModel CurrentStudent { get; set; }

        /// <summary>Model for passing Student information from the view</summary>
        public class StudentModel
        {
            /// <summary>Constructor for StudentModel</summary>
            /// <param name="name">Name of the student</param>
            /// <param name="id">ID of the student</param>
            /// <param name="quarter">Student's Starting Quarter</param>
            /// <param name="year">Student's Starting Year</param>
            /// <param name="degree">Student's Degree</param>
            public StudentModel(string name, string id, string quarter, string year, string degree)
            {
                this.Name = name;
                this.ID = id;
                this.Quarter = quarter;
                this.Year = year;
                this.Degree = degree;
            }

            /// <summary>Name of current student loaded in advising page</summary>
            public string Name { get; set; } = "NoName";

            /// <summary>ID of current student loaded in advising page</summary>
            public string ID { get; set; } = "NoID";

            /// <summary>Quarter of current student loaded in advising page</summary>
            public string Quarter { get; set; } = "NoQuarter";

            /// <summary>Year of current student loaded in advising page</summary>
            public string Year { get; set; } = "NoYear";

            /// <summary>Degree of current student loaded in advising page</summary>
            public string Degree { get; set; } = "NoDegree";
        }
        
        /// <summary>Attempts to load a student plan from the database</summary>
        /// <param name="ID">ID of student</param>
        /// <returns>true if the student was found in the database</returns>
        public static bool LoadStudent(string ID)
        {
            // Find student from database that matches ID
            // Return true or false depending on whether or not the student exists

            // For testing 
            CurrentStudent = new StudentModel("Example", ID, "Fall", "2018", "BS - Computer Science");
            return true;
        }

        /// <summary>Create a new student</summary>
        /// <param name="Student">StudentModel containing student information</param>
        public static void CreateStudent(StudentModel Student)
        {
            CurrentStudent = Student;
        }

        /// <summary>
        /// Attempts to retrieve a Student object from the database
        /// matching the CurrentStudent model
        /// </summary>
        /// <returns>Student object matching the current student</returns>
        public Student GetStudent()
        {
            if(CurrentStudent == null)
            {
                return null;
            }

            Season startingSeason;
            if (CurrentStudent.Quarter == "Winter")
                startingSeason = Season.Winter;
            else if (CurrentStudent.Quarter == "Spring")
                startingSeason = Season.Spring;
            else if (CurrentStudent.Quarter == "Summer")
                startingSeason = Season.Summer;
            else
                startingSeason = Season.Fall;
            uint startingYear = UInt32.Parse(CurrentStudent.Year);
            Quarter startingQuarter = new Quarter(startingYear, startingSeason);

            Name studentName = new Name(string.Empty, CurrentStudent.Name);
            Student DatabaseStudent = new Student(studentName, CurrentStudent.ID, startingQuarter);
            
            return DatabaseStudent;
        }

        /// <summary>
        /// AJAX handler for loading student schedule
        /// Passes Student ID to Advising model
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostRecieveScheduleModel()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var scheduleModel = JsonConvert.DeserializeObject<ScheduleModel>(requestBody);
                    CallSchedulingAlgorithm(scheduleModel);
                    return new JsonResult("Model sent succesfully.");
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }

        /// <summary>
        /// Extracts the remaining requirements from a ScheduleModel
        /// and attempts to match them to database Course objects
        /// </summary>
        /// <param name="model">ScheduleModel containing remaining requirements</param>
        /// <returns>List of remaining Course objects</returns>
        public static List<Course> ScheduleModelToRemainingRequirements(ScheduleModel model)
        {
            List<Course> RemainingRequirements = new List<Course>();

            foreach(Requirement req in model.UnmetRequirements)
            {
                Course course = ManageCoursesModel.MasterCourseList.Find(delegate (Course masterCourse) { return masterCourse.ID == req.Title; });
                if(course != null)
                {
                    RemainingRequirements.Add(course);
                }
            }

            return RemainingRequirements;
        }

        /// <summary>
        /// Extracts a Schedule from a ScheduleModel
        /// and attempts to match each course to database Coures objects
        /// </summary>
        /// <param name="model">ScheduleModel containing a schedule</param>
        /// <returns>Schedule object matching the model</returns>
        public static Schedule ScheduleModelToSchedule(ScheduleModel model)
        {
            ManageCoursesModel.GetCoursesFromDatabase();
            Schedule schedule = null;
            foreach(ModelQuarter modelQuarter in model.Quarters)
            {
                if (schedule == null)
                    schedule = new Schedule(StringToQuarter(model.Quarters[0].Title));
                else
                    schedule = schedule.NextSchedule();

                foreach (Requirement req in modelQuarter.Courses)
                {
                    Course course = ManageCoursesModel.MasterCourseList.Find(delegate (Course masterCourse) { return masterCourse.ID == req.Title; });
                    if(course != null)
                    {
                        schedule.AddCourse(course);
                    }
                }
            }
            return schedule;
        }

        /// <summary>
        /// Takes a string quarter name from the model
        /// and converts it to database Quarter objects
        /// </summary>
        /// <param name="QuarterName">string quarter name in "Season Year" format</param>
        /// <returns>Database quarter object matching the quarter name</returns>
        public static Quarter StringToQuarter(string QuarterName)
        {
            string[] quarterSplit = QuarterName.Split(' ');
            if (quarterSplit.Length != 0)
                return new Quarter(2018, Season.Fall);
            string SeasonStr = quarterSplit[0];
            uint Year = UInt32.Parse(quarterSplit[1]);

            Season season;
            if (quarterSplit[0] == "Winter")
                season = Season.Winter;
            else if (quarterSplit[0] == "Spring")
                season = Season.Spring;
            else if (quarterSplit[0] == "Summer")
                season = Season.Summer;
            else
                season = Season.Fall;

            return new Quarter(Year, season);
        }

        /// <summary>
        /// Extracts requirements, schedule, and constraints from a ScheduleModel
        /// and passes them to the scheduling algorithm
        /// </summary>
        /// <param name="model">ScheduleModel from the client</param>
        /// <returns>Schedule from the scheduling algorithm</returns>
        public static Schedule CallSchedulingAlgorithm(ScheduleModel model)
        {
            Schedule ConvertedScheduleModel = ScheduleModelToSchedule(model);
            List<Course> RemainingRequirements = ScheduleModelToRemainingRequirements(model);
            //Algorithm.Generate(RemainingRequirements, ConvertedScheduleModel, model.Constraints.MinCredits, model.Constraints.MaxCredits);
            return null;
        }
    }
}