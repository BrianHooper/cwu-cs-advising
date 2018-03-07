using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CwuAdvising.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CwuAdvising.Pages
{
    /// <summary>Model for student advising page</summary>
    public class StudentAdvisingModel : PageModel
    {
        /// <summary>Pair class for JSON degree model</summary>
        public class DegreePair
        {
            /// <summary>Constructor for pair class</summary>
            /// <param name="name">Name of the degree</param>
            /// <param name="year">Academic year</param>
            public DegreePair(string name, uint year)
            {
                this.Name = name;
                this.Year = year;
            }

            /// <summary>Name of the degree</summary>
            public string Name { get; set; }

            /// <summary>Academic year</summary>
            public uint Year { get; set; }
        }

        /// <summary>Gets a list of Degrees from the database</summary>
        /// <returns>JSON serialized list of degrees as a string</returns>
        public string GetDegrees()
        {

            List<DegreePair> DegreeList = new List<DegreePair>();
            if(!IndexModel.LoggedIn)
            {
                return JsonConvert.SerializeObject(DegreeList);
            }
            try
            {
                ManageDegreesModel.LoadDegreeModelList();
                foreach (DegreeModel model in ManageDegreesModel.ModelList)
                {
                    DegreePair Pair = new DegreePair(model.name, model.year);
                    DatabaseInterface.WriteToLog("GetDegrees loaded: " + model.year + " " + model.name);
                    DegreeList.Add(Pair);
                }
            }
            catch(Exception e)
            {
                DatabaseInterface.WriteToLog("GetDegrees exception: " + e.Message);
            }
            return JsonConvert.SerializeObject(DegreeList);
        }


        /// <summary>
        /// AJAX handler for loading student schedule
        /// Passes Student ID to Advising model
        /// </summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostLoadStudent()
        {
            if(!IndexModel.LoggedIn)
            {
                return new JsonResult(false);
            }
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var StudentId = JsonConvert.DeserializeObject<string>(requestBody);
                    if(AdvisingModel.LoadStudent(StudentId))
                    {
                        return new JsonResult(true);
                    } 
                    else
                    {
                        return new JsonResult(false);
                    }
                    
                }
                else
                {
                    return new JsonResult(false);
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
            if (!IndexModel.LoggedIn)
            {
                return new JsonResult(false);
            }
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var Student = JsonConvert.DeserializeObject<AdvisingModel.StudentModel>(requestBody);
                    bool CreateSuccess = AdvisingModel.CreateStudent(Student);
                    return new JsonResult(CreateSuccess);
                }
                else
                {
                    return new JsonResult(false);
                }
            }
        }
    }
}
