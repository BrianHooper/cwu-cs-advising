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
    /// <summary>Model for degree management</summary>
    public class ManageDegreesModel : PageModel
    {
        /// <summary>List containing the degrees loaded from the database</summary>
        public static List<DegreeModel> DegreeList { get; set; } = null;

        /// <summary>Loads degrees from the database</summary>
        /// <returns>Returns a list of DegreeModels serialized as a JSON string</returns>
        public static bool LoadDatabaseDegrees()
        {
            DegreeList = new List<DegreeModel>();

            if (Program.Database.connected)
            {
                // Load degrees from database
                return true;
            }
            else
            {
                DegreeModel DM = new DegreeModel("BS - Computer Science", 2018);
                DM.requirements.Add("CS301");
                DM.requirements.Add("CS302");
                DM.requirements.Add("CS361");
                DM.requirements.Add("CS470");
                DegreeList.Add(DM);
                DegreeList.Add(new DegreeModel("BS - Information Technology", 2018));
                DegreeList.Add(new DegreeModel("BS - Mathematics", 2018));
                return false;
            }
        }

        /// <summary>Parses the degree list</summary>
        /// <returns>DegreeModel list as a JSON string</returns>
        public static string ReadModel()
        {
            if(DegreeList != null)
            {
                return JsonConvert.SerializeObject(DegreeList);
            }
            else
            {
                return "";
            }
        }

        /// <summary>Updates database with the modified degree</summary>
        /// <param name="model">Model of the degree to be updated</param>
        /// <returns>true if the update was successful</returns>
        public static bool WriteDatabaseDegree(DegreeModel model)
        {
            return false;
        }

        /// <summary>Retrieves a modified degree as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendDegree()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var ModifiedDegree = JsonConvert.DeserializeObject<DegreeModel>(requestBody);
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