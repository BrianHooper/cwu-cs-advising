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
    public class ManageCoursesModel : PageModel
    {
        public static CourseModel RecievedCourses = null;

        public List<CourseModel> GetCoursesFromServer()
        {
            List<CourseModel> CourseList = new List<CourseModel>();

            // For Testing
            Course CS301 = new Course("Data Structures I", "CS 301", 4, false);
            CourseList.Add(ParseDatabaseCourse(CS301));
            Course CS302 = new Course("Data Structures II", "CS 302", 4, false);
            CS302.AddPreRequisite(CS301);
            CourseList.Add(ParseDatabaseCourse(CS302));
            Course CS470 = new Course("Operating Systems", "CS 470", 4, false);
            CS470.AddPreRequisite(new Course("Prog. Language Design", "CS 362", 4, false));
            CourseList.Add(ParseDatabaseCourse(CS470));
            
            return CourseList;
        }

        public CourseModel ParseDatabaseCourse(Course DatabaseCourse)
        {
            CourseModel ModelCourse = new CourseModel();
            ModelCourse.Title = DatabaseCourse.Name;
            ModelCourse.Number = DatabaseCourse.ID;
            ModelCourse.Department = "Computer Science";
            ModelCourse.Offered = "124";
            ModelCourse.Credits = DatabaseCourse.Credits.ToString();
            for(int i = 0; i < DatabaseCourse.PreRequisites.Count; i++)
            {
                ModelCourse.PreReqs.Add(DatabaseCourse.PreRequisites[i].ID);
            }

            return ModelCourse;
        }

        public string CourseListAsJson()
        {
            if(RecievedCourses != null)
            {
                return JsonConvert.SerializeObject(RecievedCourses);
            }
            return JsonConvert.SerializeObject(GetCoursesFromServer());
        }

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
                    var obj = JsonConvert.DeserializeObject<List<CourseModel>>(requestBody);
                    return new JsonResult("Courses saved succesfully.");
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }
    }
}