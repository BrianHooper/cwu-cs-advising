using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CwuAdvising.Pages
{
    /// <summary>Model for student advising page</summary>
    public class StudentAdvisingModel : PageModel
    {
        /// <summary>Gets a list of Degrees from the database</summary>
        /// <returns>JSON serialized list of degrees as a string</returns>
        public string GetDegrees()
        {
            List<string> Degrees = new List<string>
            {
                "BS - Computer Science",
                "BS - Information Technology",
                "BS - Mathematics"
            };

            return JsonConvert.SerializeObject(Degrees);
        }


        /// <summary>
        /// AJAX handler for loading student schedule
        /// Passes Student ID to Advising model
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostLoadStudent()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var StudentId = JsonConvert.DeserializeObject<string>(requestBody);
                    AdvisingModel.LoadStudent(StudentId);
                    return new JsonResult("Users saved succesfully.");
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }

        /// <summary>
        /// AJAX handler for creating new student schedule
        /// Passes Student Information to Advising model
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostCreateStudent()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var Student = JsonConvert.DeserializeObject<AdvisingModel.StudentModel>(requestBody);
                    AdvisingModel.CreateStudent(Student);
                    return new JsonResult("Users saved succesfully.");
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }
    }
}
