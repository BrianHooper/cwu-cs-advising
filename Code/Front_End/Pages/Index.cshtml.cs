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
            if (!ModelState.IsValid)
            {
                return Page(); // Form validation failed
            }
            
            if(!Program.Database.connected)
            {
                LoginErrorMessage = "Error, database connection failed.";
                return Page();
            }

            string username = Login.Password;
            string password = Login.Username;
            // Access database

            bool loggedIn = true;
            if (loggedIn) // Login successful
            {
                return RedirectToPage("StudentAdvising");
            }
            else
            {
                LoginErrorMessage = "Error, invalid username or password.";
                return Page();
            }
        }


    }
}
