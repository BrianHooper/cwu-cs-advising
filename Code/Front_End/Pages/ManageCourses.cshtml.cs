using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CwuAdvising.Models;
using Newtonsoft.Json;
using Database_Object_Classes;
using System.IO;

namespace CwuAdvising.Pages
{
    /// <summary>
    /// Model for managing coures in database
    /// </summary>
    public class ManageCoursesModel : PageModel
    {
        public static string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Converts Course list to CourseModel list
        /// </summary>
        /// <param name="CourseList">List of Course objects</param>
        /// <returns>List of CourseModel objects</returns>
        public static List<CourseModel> CourseListToCourseModelList(List<Course> CourseList)
        {
            List<CourseModel> ModelList = new List<CourseModel>();

            foreach(Course course in CourseList)
            {
                CourseModel model = CourseModel.Convert(course);
                if(!course.IsShallow) {
                    foreach (Course prereq in course.PreRequisites)
                    {
                        model.PreReqs.Add(prereq.ID);
                    }
                } else
                {
                    foreach (string prereq in course.ShallowPreRequisites)
                    {
                        model.PreReqs.Add(prereq);
                    }
                }
                ModelList.Add(model);
            }

            return ModelList;
        }

        /// <summary>
        /// Converts CourseModel list to Course list
        /// </summary>
        /// <param name="ModelList">List of CourseModel objects</param>
        /// <returns>List of Course objects</returns>
        public List<Course> GetCoursesToDelete(List<CourseModel> ModelList)
        {
            List<Course> CourseList = new List<Course>();

            foreach (CourseModel model in ModelList)
            {
                if (model.Delete)
                {
                    Course c = (Course) model;
                    CourseList.Add(c);
                }
            }

            return CourseList;
        }

        /// <summary>
        /// Converts CourseModel list to Course list
        /// </summary>
        /// <param name="ModelList">List of CourseModel objects</param>
        /// <returns>List of Course objects</returns>
        public List<Course> GetCoursesToUpdate(List<CourseModel> ModelList)
        {
            List<Course> CourseList = new List<Course>();

            foreach (CourseModel model in ModelList)
            {
                if(!model.Delete && model != null)
                {
                    bool[] offered = new bool[4];

                    for (int i = 0; i < model.Offered.Length; i++)
                    {
                        switch (model.Offered[i])
                        {
                            case '1':
                                offered[0] = true;
                                break;
                            case '2':
                                offered[1] = true;
                                break;
                            case '3':
                                offered[2] = true;
                                break;
                            case '4':
                                offered[3] = true;
                                break;
                        } // end switch
                    } // end for

                    if (model.Credits == null)
                    {
                        model.Credits = "0";
                    }

                    Course course = new Course("", model.ID, uint.Parse(model.Credits), false, offered);
                    
                    if(model.Name != null)
                    {
                        course.Name = model.Name;
                    } else
                    {
                        course.Name = "";
                    }

                    if (model.Department != null)
                    {
                        course.Department = model.Department;
                    }
                    else
                    {
                        course.Department = "no department";
                    }

                    foreach (string prereq in model.PreReqs)
                    {
                        Course prereqCourse = new Course("", prereq, 0, false);
                        prereqCourse.Department = "";
                        course.AddPreRequisite(prereqCourse);
                    }

                    CourseList.Add(course);
                }
            }

            return CourseList;
        }

        /// <summary>
        /// Updates MasterCourseList from database and
        /// Serializes CourseModel list to a JSON string.
        /// </summary>
        /// <returns>CourseModel list as serialized JSON string</returns>
        public static string CourseListAsJson()
        {
            Program.DbObjects.GetCoursesFromDatabase(true);
            List<CourseModel> ModelList = CourseListToCourseModelList(Program.DbObjects.MasterCourseList);
            return JsonConvert.SerializeObject(ModelList);
        }
        
        /// <summary>Retrieves a list of modified courses as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendCourses()
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                Request.Body.CopyTo(stream);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string requestBody = reader.ReadToEnd();
                    if (requestBody.Length > 0)
                    {
                        var ModifiedCourses = JsonConvert.DeserializeObject<List<CourseModel>>(requestBody);
                        
                        List<Course> CoursesToUpdate = GetCoursesToUpdate(ModifiedCourses);
                        //List<Course> CoursesToDelete = GetCoursesToDelete(ModifiedCourses);
                        /*
                        if (!DeleteDatabaseCourses(CoursesToDelete))
                        {
                            return new JsonResult("Course update failed.");
                        }
                        */
                        if (!UpdateDatabaseCourses(CoursesToUpdate))
                        {
                            return new JsonResult("Course update failed.");
                        }
                        
                        return new JsonResult("Courses saved!");
                    }
                    else
                    {
                        return new JsonResult("Error passing data to server.");
                    }
                }
            } catch(Exception e)
            {
                System.IO.File.AppendAllText("wwwroot/log.txt", e.Message);
                return new JsonResult(e.Message);
            }
            
        }

        private bool UpdateDatabaseCourses(List<Course> CourseList)
        {
            /*
            if(!Program.Database.connected)
            {
                return false;
            }
            */
            foreach(Course c in CourseList)
            {
                if(!Program.Database.UpdateRecord(c, Database_Handler.OperandType.Course))
                {
                    return false;
                }
            }
            return true;
        }

        private bool DeleteDatabaseCourses(List<Course> CourseList)
        {
            if (!Program.Database.connected)
            {
                return false;
            }

            foreach (Course c in CourseList)
            {
                if (!Program.Database.DeleteRecord(c, Database_Handler.OperandType.Course))
                {
                    return false;
                }
            }
            return true;
        }
    }
}