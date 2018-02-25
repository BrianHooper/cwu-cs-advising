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
        /// <summary>Loads degrees from the database</summary>
        /// <returns>Returns a list of DegreeModels serialized as a JSON string</returns>
        public static string LoadDatabaseDegrees()
        {
            List<DegreeModel> model = new List<DegreeModel>();
            
            DegreeModel DM = new DegreeModel("BS - Computer Science", 2018);
            DM.requirements.Add("CS301");
            DM.requirements.Add("CS302");
            DM.requirements.Add("CS361");
            DM.requirements.Add("CS470");
            model.Add(DM);
            model.Add(new DegreeModel("BS - Information Technology", 2018));
            model.Add(new DegreeModel("BS - Mathematics", 2018));
            
            return JsonConvert.SerializeObject(model);
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