using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Database_Object_Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CwuAdvising.Pages
{
    /// <summary>Model for index page</summary>
    public class IndexModel : PageModel
    {
        /// <summary>Error message to display to the user on failed login</summary>
        public static string LoginErrorMessage = "";

        /// <summary>Status of database connection to display to the user</summary>
        public static string DatabaseConnectionMessage = "";

        /// <summary></summary>
        public class LoginModel
        {
            /// <summary>Username for login</summary>
            [Required]
            public string Username { get; set; }

            /// <summary>Password for login</summary>
            [Required]
            public string Password { get; set; }
        }

        /// <summary>Binds the LoginModel to a Login object for POST</summary>
        [BindProperty]
        public LoginModel Login { get; set; }
        
        /// <summary>Recieve login information</summary>
        /// <returns>Redirects to StudentAdvising if logging in is successful</returns>
        public IActionResult OnPost()
        {
            string password = Login.Password;
            string username = Login.Username;

            if (!ModelState.IsValid)
            {
                return Page(); // Form validation failed
            }

            /*
            if(!Program.Database.connected)
            {
                LoginErrorMessage = "Error, database connection failed.";
                return Page();
            }
            */

            // Access database

            int error_code = PasswordManager.LoginAttempt(username, password);

            switch(error_code)
            {
                case -2:
                    //LoginErrorMessage = "Connection to the database could not be established.";
                    return Page();
                case 0:
                    return RedirectToPage("StudentAdvising");
                case 1:
                    LoginErrorMessage = "Advisor - You must change your password.";
                    return Page();
                case 2:
                    return RedirectToPage("ManageCourses");
                case 3:
                    LoginErrorMessage = "Admin - You must change you password.";
                    return Page();
                default:
                    LoginErrorMessage = "Error, invalid username or password.";
                    return Page();
            } // end switch
        }


    }
}
