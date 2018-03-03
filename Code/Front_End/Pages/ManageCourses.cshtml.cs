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
                foreach(Course prereq in course.PreRequisites)
                {
                    model.PreReqs.Add(prereq.ID);
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

                    foreach (string prereq in model.PreReqs)
                    {
                        c.AddPreRequisite(Program.DbObjects.MasterCourseList.Find(delegate (Course masterCourse) { return masterCourse.ID == prereq; }));
                    }

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
                if(!model.Delete)
                {
                    Course c = (Course)model;

                    foreach (string prereq in model.PreReqs)
                    {
                        c.AddPreRequisite(Program.DbObjects.MasterCourseList.Find(delegate (Course masterCourse) { return masterCourse.ID == prereq; }));
                    }

                    CourseList.Add(c);
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
            Program.DbObjects.GetCoursesFromDatabase();
            List<CourseModel> ModelList = CourseListToCourseModelList(Program.DbObjects.MasterCourseList);
            return JsonConvert.SerializeObject(ModelList);
        }
        
        /// <summary>Retrieves a list of modified courses as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendCourses()
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
                    /*
                    List<Course> CoursesToDelete = GetCoursesToDelete(ModifiedCourses);
                    if(!UpdateDatabaseCourses(CoursesToDelete))
                    {
                        return new JsonResult("Course update failed.");
                    }
                    */
                    if(!UpdateDatabaseCourses(CoursesToUpdate))
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
        }

        private bool UpdateDatabaseCourses(List<Course> CourseList)
        {
            if(!Program.Database.connected)
            {
                return false;
            }

            foreach(Course c in CourseList)
            {
                if(!Program.Database.UpdateRecord(c, Database_Handler.OperandType.Course))
                {
                    return false;
                }
            }
            return true;
        }
    }
}