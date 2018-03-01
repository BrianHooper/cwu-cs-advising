using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CwuAdvising.Models;
using Database_Object_Classes;
using Newtonsoft.Json;
using System.IO;

namespace CwuAdvising.Pages
{
    /// <summary>Model for User Management page</summary>
    public class UserManagementModel : PageModel
    {
        /// <summary>Stores a list of credentials retireved from the database</summary>
        public static List<Credentials> MasterUserList;

        /// <summary>Read all credentials from the database to the MasterUserList</summary>
        public void ReadDatabase()
        {
            List<Credentials> UserList = new List<Credentials>();
            UserList.Add(new Credentials("root", 0, true, true, new byte[] { 0x20, 0x20 }, ""));
            UserList.Add(new Credentials("user1", 0, false, true, new byte[] { 0x20, 0x20 }, ""));
            UserList.Add(new Credentials("user2", 0, true, false, new byte[] { 0x20, 0x20 }, ""));
            MasterUserList = UserList;
        }

        /// <summary>Convert a list of Credentials to a list of UserModels</summary>
        /// <param name="CredentialsList">List of Credentials objects</param>
        /// <returns>List of UserModel objects</returns>
        public List<UserModel> CredentialsListToUserModelList(List<Credentials> CredentialsList)
        {
            List<UserModel> UserModelList = new List<UserModel>();
            foreach (Credentials cred in CredentialsList)
            {
                UserModelList.Add(new UserModel(cred.UserName, cred.IsAdmin, false, cred.IsActive));
            }

            return UserModelList;
        }

        /// <summary>Converts a list of UserModels to a list of Credentials</summary>
        /// <param name="UserModelList">List of UserModel objects</param>
        /// <returns>List of Credentials objects</returns>
        public List<Credentials> UserModelListToCredentials(List<UserModel> UserModelList)
        {
            List<Credentials> CredentialsList = new List<Credentials>();

            foreach(UserModel model in UserModelList)
            {
                CredentialsList.Add((Credentials)model);
            }

            return CredentialsList;
        }

        /// <summary>
        /// Updates MasterCourseList from database and
        /// Serializes CourseModel list to a JSON string.
        /// </summary>
        /// <returns>CourseModel list as serialized JSON string</returns>
        public string UserListAsJson()
        {
            ReadDatabase();
            List<UserModel> ModelList = CredentialsListToUserModelList(MasterUserList);
            return JsonConvert.SerializeObject(ModelList);
        }

        /// <summary>Retrieves a list of modified courses as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendUsers()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var ModifiedUsers = JsonConvert.DeserializeObject<List<UserModel>>(requestBody);
                    var DatabaseUpdate = UserModelListToCredentials(ModifiedUsers);

                    if(Program.Database.connected)
                    {
                        foreach (Credentials user in DatabaseUpdate)
                        {
                            Program.Database.UpdateRecord(user);
                        }
                    }

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