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
using System.ComponentModel.DataAnnotations;

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
            List<Credentials> UserList = Program.Database.GetAllUsers();

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
            if (!IndexModel.LoggedIn || !IndexModel.Administrator)
            {
                UserModel testModel = new UserModel("brian", true, false, true);
                List<UserModel> userList = new List<UserModel>();
                userList.Add(testModel);
                return JsonConvert.SerializeObject(userList);
            }
            ReadDatabase();
            List<UserModel> ModelList = CredentialsListToUserModelList(MasterUserList);
            return JsonConvert.SerializeObject(ModelList);
        }

        /// <summary>Retrieves a list of modified courses as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostChangePassword()
        {
            /*
            if (!IndexModel.LoggedIn)
            {
                return new JsonResult("Error saving users.");
            }
            */

            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    //ChangePasswordModel.Username = requestBody;
                    return new JsonResult(true);
                }
                else
                {
                    return new JsonResult(false);
                }
            }

        }

        /// <summary>Retrieves a list of modified courses as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendUsers()
        {
            if(!IndexModel.LoggedIn)
            {
                return new JsonResult("Error saving users.");
            }
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
        /// <summary></summary>
        public class CreateUserModel
        {
            /// <summary>Username</summary>
            [Required]
            public string Username { get; set; }

            /// <summary>Password 1</summary>
            [Required]
            public string PasswordOne { get; set; }

            /// <summary>Password 2</summary>
            [Required]
            public string PasswordTwo { get; set; }
        }

        /// <summary>Error message for Create User form</summary>
        public static string CreateUserErrorMessage { get; set; } = "";

        /// <summary>Binds the LoginModel to a Login object for POST</summary>
        [BindProperty]
        public CreateUserModel CreateUser { get; set; }

        /// <summary>Recieve login information</summary>
        /// <returns>Redirects to StudentAdvising if logging in is successful</returns>
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || !IndexModel.LoggedIn || !IndexModel.Administrator)
            {
                return Page(); // Form validation failed
            }

            string username = CreateUser.Username;
            string password1 = CreateUser.PasswordOne;
            string password2 = CreateUser.PasswordTwo;

            if (password1 != password2)
            {
                CreateUserErrorMessage = "Error, passwords do not match.";
                return Page();
            }
            else if (password1.Length < 10)
            {
                CreateUserErrorMessage = "Error, password too short.";
                return Page();
            }
            DatabaseInterface.WriteToLog("Attempting to create user " + username);
            Credentials user = new Credentials(username, 0, false, false, new byte[32], "");

            DatabaseInterface.WriteToLog("Attempting to add user " + user.UserName + " to database");
            bool success = Program.Database.UpdateRecord(user);

            if(!success)
            {
                // If username is already taken
                CreateUserErrorMessage = "Error, invalid username or password.";
                return Page();
            }

            success = PasswordManager.ChangePassword(username, password1);

            if (!success)
            {
                // If username is already taken
                CreateUserErrorMessage = "Error, invalid username or password.";
            }
            else
            {
                CreateUserErrorMessage = "Success!";
            }

            return Page();
        }


    }
}