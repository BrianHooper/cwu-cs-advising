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

        /// <summary>Current students schedule as JSON string</summary>
        public static ScheduleModel CurrentSchedule { get; set; }

        /// <summary>Model for passing Student information from the view</summary>
        public class StudentModel
        {
            /// <summary>Constructor for StudentModel</summary>
            /// <param name="name">Name of the student</param>
            /// <param name="id">ID of the student</param>
            /// <param name="quarter">Student's Starting Quarter</param>
            /// <param name="year">Student's Starting Year</param>
            /// <param name="degree">Student's Degree</param>
            /// <param name="catalogyear">Student's catalog year</param>
            public StudentModel(string name, string id, string quarter, string year, string degree, string catalogyear)
            {
                this.Name = name;
                this.ID = id;
                this.Quarter = quarter;
                this.Year = year;
                this.Degree = degree;
                this.AcademicYear = catalogyear;
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

            /// <summary>Catalog year for the students degree</summary>
            public string AcademicYear { get; set; } = "NoCatalogYear";
        }

        /// <summary>Gets the base case for the student's degree</summary>
        /// <returns>Schedule as a JSON string</returns>
        public static string LoadBaseCase()
        {
            if(!Program.Database.connected || !IndexModel.LoggedIn)
            {
                return System.IO.File.ReadAllText("wwwroot/ExamplePlan.json");
            }
            else
            {
                DatabaseInterface.WriteToLog("Attempting to retrieve basecase ");
                PlanInfo plantemplate = new PlanInfo("-2", 0, Quarter.DefaultQuarter.ToString(), new string[1]);
                PlanInfo dbschedule = Program.Database.RetrieveRecord(plantemplate);
                DatabaseInterface.WriteToLog("Retrieved planinfo " + dbschedule.StudentID);
                return dbschedule.Classes[0];
                
            }
        }
        
        /// <summary>Loads the schedule plan for the student</summary>
        /// <param name="ID">The students unique ID</param>
        /// <returns>Schedule as a JSON string</returns>
        public static string LoadStudentSchedule(string ID)
        {
            if(!Program.Database.connected)
            {
                return "";
            }
            else
            {
                PlanInfo template = new PlanInfo(ID, 0, Quarter.DefaultQuarter.ToString(), new string[1]);
                PlanInfo dbschedule = Program.Database.RetrieveRecord(template);
                return dbschedule.Classes[0];
            }
        }

        /// <summary>Attempts to load a student plan from the database</summary>
        /// <param name="ID">ID of student</param>
        /// <returns>true if the student was found in the database</returns>
        public static bool LoadStudent(string ID)
        {


            CurrentStudent = new StudentModel("Example", ID, "Fall", "2018", "BS - Computer Science", "2018");
            if(!Program.Database.connected)
            {
                return true;
            }
            else
            {
                try
                {
                    Student template = new Student(Name.DefaultName, ID, Quarter.DefaultQuarter);

                    DatabaseInterface.WriteToLog("Attempting to retrieve student " + ID);
                    Student dbstudent = (Student)Program.Database.RetrieveRecord(template, Database_Handler.OperandType.Student);
                    if(dbstudent == null)
                    {
                        DatabaseInterface.WriteToLog("RetrieveRecord retrieved a null student in LoadStudent");
                        return false;
                    }
                    DatabaseInterface.WriteToLog("Retrieved student " + dbstudent.ID);
                    
                    DatabaseInterface.WriteToLog("Attempting to retrieve planinfo " + ID);
                    PlanInfo plantemplate = new PlanInfo(ID, 0, Quarter.DefaultQuarter.ToString(), new string[1]);
                    PlanInfo dbschedule = Program.Database.RetrieveRecord(plantemplate);
                    if(dbschedule.StudentID == null)
                    {
                        DatabaseInterface.WriteToLog("RetrieveRecord retrieved a blank schedule in LoadStudent");
                        return false;
                    }
                    DatabaseInterface.WriteToLog("Retrieved planinfo " + dbschedule.StudentID);
                    CurrentSchedule = JsonConvert.DeserializeObject<ScheduleModel>(dbschedule.Classes[0]);
                    
                    CurrentStudent = new StudentModel(
                        dbstudent.Name.ToString(),
                        dbstudent.ID,
                        dbstudent.StartingQuarter.QuarterSeason.ToString(),
                        dbstudent.StartingQuarter.Year.ToString(),
                        CurrentSchedule.Name,
                        CurrentSchedule.AcademicYear);
                } catch(Exception e)
                {
                    DatabaseInterface.WriteToLog("LoadStudent threw exception " + e.Message + " when trying to load student " + ID);
                    return false;
                }
                
            }
            return true;
        }

        /// <summary>Create a new student</summary>
        /// <param name="model">StudentModel containing student information</param>
        public static bool CreateStudent(StudentModel model)
        {
            try
            {

                if (Program.Database.connected)
                {
                    DatabaseInterface.WriteToLog("Attempting to create student: " + model.ID + " " + model.Name + " " + model.Quarter + " " + model.Year + " " + model.Degree + " " + model.AcademicYear);
                    Student CreatedStudent = new Student(new Name(model.Name, ""), model.ID, new Quarter(uint.Parse(model.Year), (Season) Enum.Parse(typeof(Season), model.Quarter)));

                    DatabaseInterface.WriteToLog("Created student object " + CreatedStudent.ToString());
                    if (Program.Database.UpdateRecord(CreatedStudent, Database_Handler.OperandType.Student))
                    {
                        DatabaseInterface.WriteToLog("Added student to database: " + CreatedStudent.ID);
                    }
                    else
                    {
                        DatabaseInterface.WriteToLog("Database command UpdateRecord failed for creating student: " + CreatedStudent.ID);
                        return false;
                    }
                    CurrentStudent = model;

                    DatabaseInterface.WriteToLog("Attempting to create new student plan: " + CreatedStudent.ID);
                    ScheduleModel CreatedScheduleModel = new ScheduleModel
                    {
                        Name = model.Degree,
                        AcademicYear = model.AcademicYear,
                        Quarters = new List<ModelQuarter>()
                    };
                    ModelQuarter StartingQuarter = new ModelQuarter
                    {
                        Courses = new List<Requirement>(),
                        Locked = false,
                        Title = model.Quarter
                    };
                    CreatedScheduleModel.Quarters.Add(StartingQuarter);

                    DatabaseInterface.WriteToLog("Attempting to retrieve degree requirements for degree: " + CreatedScheduleModel.Name + ", academic year " + CreatedScheduleModel.AcademicYear);
                    CatalogRequirements template = new CatalogRequirements(model.AcademicYear, new List<DegreeRequirements>());
                    CatalogRequirements Catalog = (CatalogRequirements)Program.Database.RetrieveRecord(template, Database_Handler.OperandType.CatalogRequirements);
                    if (Catalog == null)
                    {
                        DatabaseInterface.WriteToLog("Load catalog failed for academic year " + CreatedScheduleModel.AcademicYear);
                        return false;
                    }
                    DatabaseInterface.WriteToLog("Searching catalog " + Catalog.ID + " for degree " + model.Degree);
                    Object temp = Catalog.DegreeRequirements.ToList().Find(delegate (DegreeRequirements degree) { return degree.Name == model.Degree; });
                    if (temp == null)
                    {
                        DatabaseInterface.WriteToLog("Searching catalog " + Catalog.ID + " for degree " + model.Degree + " returned null");
                        return false;
                    }
                    DegreeRequirements Degree = (DegreeRequirements)temp;

                    DatabaseInterface.WriteToLog("Attempting to load degree requirements from " + Degree.ID + " to " + CreatedScheduleModel.Name);
                    CreatedScheduleModel.UnmetRequirements = new List<Requirement>();
                    foreach (Course course in Degree.Requirements)
                    {
                        DatabaseInterface.WriteToLog("Converting course " + course.ID + " to requirement");
                        Requirement req = Requirement.CourseToRequirement(course);
                        CreatedScheduleModel.UnmetRequirements.Add(req);
                    }
                    DatabaseInterface.WriteToLog("Create PlanInfo object for student " + model.ID);
                    string Plan = JsonConvert.SerializeObject(CreatedScheduleModel);
                    Plan = Plan.Replace('"', '\"');
                    string[] StudentsPlan = { Plan };
                    PlanInfo CreatedPlanInfo = new PlanInfo(model.ID, 0, model.Quarter + " " + model.Year, StudentsPlan);

                    DatabaseInterface.WriteToLog("Attempting to update student plan " + CreatedPlanInfo.StudentID);
                    if (Program.Database.UpdateRecord(CreatedPlanInfo))
                    {
                        DatabaseInterface.WriteToLog("Successfully updated student plan " + CreatedPlanInfo.StudentID);
                        return true;
                    }
                    else
                    {
                        DatabaseInterface.WriteToLog("Update student plan failed for plan " + CreatedPlanInfo.StudentID);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                DatabaseInterface.WriteToLog("CreateStudent threw exception " + e.Message);
                return false;
            }
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

        /// <summary>Loads a student plan from the database</summary>
        /// <returns>Student Schedule as a parsed JSON string</returns>
        public static string GetStudentPlan()
        {
            
            try
            {
                if (!IndexModel.LoggedIn || !Program.Database.connected || CurrentStudent == null || CurrentSchedule == null)
                {
                    DatabaseInterface.WriteToLog("GetStudentPlan returned blank schedule because student or schedule is null");
                    return JsonConvert.SerializeObject(new ScheduleModel());
                }
                else
                {
                    return JsonConvert.SerializeObject(CurrentSchedule);
                }
            }
            catch(Exception e)
            {
                DatabaseInterface.WriteToLog("GetStudentPlan threw exception " + e.Message);
                return JsonConvert.SerializeObject(new ScheduleModel());
            }
        }

        /// <summary>Saves the student plan to the database</summary>
        /// <param name="JsonPlan">Students schedule as a JSON string</param>
        /// <returns>True if the database update was successful</returns>
        public static bool SaveStudentPlan(string JsonPlan)
        {
            try
            {
                DatabaseInterface.WriteToLog("Attempting to save plan for student " + CurrentStudent.ID);
                JsonPlan = JsonPlan.Replace('"', '\"');
                string[] databasePlanSchedule = { JsonPlan };
                PlanInfo studentplan = new PlanInfo(CurrentStudent.ID, 0, CurrentStudent.Quarter + " " + CurrentStudent.Year, databasePlanSchedule);
                Program.Database.UpdateRecord(studentplan);
                return true;
            } catch(Exception e)
            {
                DatabaseInterface.WriteToLog("SaveStudentPlan threw exception " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// AJAX handler for loading student schedule
        /// Passes Student ID to Advising model
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostRecieveScheduleForAlgorithm()
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

                    DatabaseInterface.WriteToLog("Deserialized schedule from advising page for degree " + scheduleModel.Name);
                    ScheduleModel GeneratedSchedule = CallSchedulingAlgorithm(scheduleModel);

                    
                    string JsonSchedule = JsonConvert.SerializeObject(GeneratedSchedule);
                    return new JsonResult(JsonSchedule);
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }
        
        /// <summary>
        /// AJAX handler for saving student schedule
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostRecieveScheduleForSavingStudent()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    //var scheduleModel = JsonConvert.DeserializeObject<ScheduleModel>(requestBody);
                    return new JsonResult(SaveStudentPlan(requestBody));
                }
                else
                {
                    return new JsonResult(false);
                }
            }
        }

        /// <summary>Saves the base case to the database</summary>
        /// <param name="basecase">JSON string representing the base case</param>
        /// <returns>True if the database update was successful</returns>
        public static bool SaveBaseCase(String basecase)
        {
            try
            {
                DatabaseInterface.WriteToLog("Attempting to save plan for student base case");
                basecase = basecase.Replace('"', '\"');
                string[] databasePlanSchedule = { basecase };
                PlanInfo studentplan = new PlanInfo("-1", 0, "Fall 2018", databasePlanSchedule);
                Program.Database.UpdateRecord(studentplan);
                return true;
            }
            catch (Exception e)
            {
                DatabaseInterface.WriteToLog("SaveBaseCase threw exception " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// AJAX handler for saving degree base case
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostRecieveScheduleForSavingBaseCase()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    return new JsonResult(SaveBaseCase(requestBody));
                }
                else
                {
                    return new JsonResult(false);
                }
            }
        }

        /// <summary>
        /// AJAX handler for printing student schedule
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostRecieveScheduleForPrint()
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
                    CurrentSchedule = scheduleModel;
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
        public static List<Course> ScheduleModelToRemainingRequirements(ScheduleModel model, List<Course> CourseList)
        {
            List<Course> RemainingRequirements = new List<Course>();

            foreach(Requirement req in model.UnmetRequirements)
            {
                Course course = CourseList.Find(delegate (Course masterCourse) {
                    return masterCourse.ID == req.Title;
                });
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
        public static Schedule ScheduleModelToSchedule(ScheduleModel model, List<Course> CourseList)
        {
            DatabaseInterface.WriteToLog("Converting model starting from " + model.Quarters[0].Title);
            Schedule schedule = null;
            foreach(ModelQuarter modelQuarter in model.Quarters)
            {
                if (schedule == null)
                {
                    schedule = new Schedule(StringToQuarter(model.Quarters[0].Title));
                }
                else
                {
                    schedule = schedule.NextScheduleSimple();
                }

                if(modelQuarter.Locked)
                {
                    schedule.locked = true;
                }

                DatabaseInterface.WriteToLog("Adding courses to quarter " + schedule.quarterName);
                foreach (Requirement req in modelQuarter.Courses)
                {
                    Course course = CourseList.Find(delegate (Course masterCourse) {
                        return masterCourse.ID == req.Title;
                    });
                    if(course != null)
                    {
                        schedule.courses.Add(course);
                        schedule.ui_numberCredits += course.Credits;
                    }
                    else
                    {
                        DatabaseInterface.WriteToLog("Could not find course " + req.Title + " in database");
                    }
                }
                DatabaseInterface.WriteToLog("Retrieved " + schedule.courses.Count + " from quarter " + schedule.quarterName);
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
            DatabaseInterface.WriteToLog("Splitting string " + QuarterName);
            string[] quarterSplit = QuarterName.Split(' ');
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
        /// <returns>ScheduleModel from the scheduling algorithm</returns>
        public static ScheduleModel CallSchedulingAlgorithm(ScheduleModel model)
        {
            try
            {
                List<Course> DatabaseCourses = Program.Database.GetAllCourses(false);
                DatabaseInterface.WriteToLog("Converting ScheduleModel containing " + model.Quarters.Count + " quarters to Schedule");
                Schedule ConvertedScheduleModel = ScheduleModelToSchedule(model, DatabaseCourses);
                DatabaseInterface.WriteToLog("Converted to Schedule containing " + ConvertedScheduleModel.NumberOfQuarters + " quarters");
                List<Course> RemainingRequirements = ScheduleModelToRemainingRequirements(model, DatabaseCourses);
                DatabaseInterface.WriteToLog("Extracted " + RemainingRequirements.Count + " remaining requirements from the model");

                ConvertedScheduleModel = ConvertedScheduleModel.GetFirstSchedule();

                Schedule scheduleCopy = ConvertedScheduleModel;
                while(scheduleCopy != null)
                {
                    DatabaseInterface.WriteToLog("----" + scheduleCopy.quarterName.ToString() + "----");
                    foreach(Course course in scheduleCopy.courses)
                    {
                        DatabaseInterface.WriteToLog(scheduleCopy.quarterName.ToString() + "\t" + course.ID);
                        if(!course.IsShallow && course.PreRequisites != null)
                        {
                            foreach(Course prereq in course.PreRequisites)
                            {
                                DatabaseInterface.WriteToLog(scheduleCopy.quarterName.ToString() + "\t" + course.ID + " has prereq " + prereq.ID);
                            }
                        }
                    }
                    scheduleCopy = scheduleCopy.NextQuarter;
                }
                
                Schedule GeneratedSchedule = Algorithm.Generate(RemainingRequirements, ConvertedScheduleModel, 
                    model.Constraints.MinCredits, model.Constraints.MaxCredits, model.Constraints.TakingSummer);

                DatabaseInterface.WriteToLog("Finished algorithm");

                GeneratedSchedule = GeneratedSchedule.GetFirstSchedule();
                scheduleCopy = GeneratedSchedule;
                while (scheduleCopy != null)
                {
                    DatabaseInterface.WriteToLog("----" + scheduleCopy.quarterName.ToString() + "----");
                    foreach (Course course in scheduleCopy.courses)
                    {
                        DatabaseInterface.WriteToLog(scheduleCopy.quarterName.ToString() + "\t" + course.ID);
                        if (!course.IsShallow && course.PreRequisites != null)
                        {
                            foreach (Course prereq in course.PreRequisites)
                            {
                                DatabaseInterface.WriteToLog(scheduleCopy.quarterName.ToString() + "\t" + course.ID + " has prereq " + prereq.ID);
                            }
                        }
                    }
                    scheduleCopy = scheduleCopy.NextQuarter;
                }
                
                ScheduleModel GeneratedModel = ScheduleToScheduleModel(GeneratedSchedule, model);
                return GeneratedModel;
            }
            catch(Exception e)
            {
                DatabaseInterface.WriteToLog("CallSchedulingAlgorithm caught exception: " + e.Message);
                return new ScheduleModel();
            }
        }


        /// <summary>Converts a schedule object, used by the algorithm, to a ScheduleModel object. </summary>
        /// <param name="schedule"></param>
        /// <param name="oldModel"></param>
        /// <returns></returns>
        public static ScheduleModel ScheduleToScheduleModel(Schedule schedule, ScheduleModel oldModel)
        {
            if(schedule.previousQuarter != null)
            {
                do
                {
                    schedule = schedule.previousQuarter;
                } while (schedule.previousQuarter != null);
            }


            ScheduleModel model = new ScheduleModel
            {
                Constraints = oldModel.Constraints,
                UnmetRequirements = new List<Requirement>()
            };

            List<ModelQuarter> QuarterList = new List<ModelQuarter>();
            while (schedule != null)
            {
                ModelQuarter quarter = new ModelQuarter
                {
                    Title = schedule.quarterName.ToString(),
                    Locked = schedule.locked,
                    Courses = new List<Requirement>()
                };

                foreach (Course course in schedule.courses)
                {
                    quarter.Courses.Add(Requirement.CourseToRequirement(course));
                }
                QuarterList.Add(quarter);
                schedule = schedule.NextQuarter;
            }

            model.Quarters = QuarterList;

            return model;
        }
    }
}