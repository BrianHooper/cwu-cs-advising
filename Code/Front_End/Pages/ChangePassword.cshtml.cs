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
    public class ChangePasswordModel : PageModel
    {
        public static string Username = "-1";


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || !IndexModel.LoggedIn)
            {
                return Page(); // Form validation failed
            }

            if (UpdatedUser.NewPasswordOne != UpdatedUser.NewPasswordTwo)
            {
                ChangePasswordErrorMessage = "Your new passwords do not match!";
                return Page();
            }

            int temp = PasswordManager.LoginAttempt(Username, UpdatedUser.OldPassword);

            if (temp >= 0)
            {
                bool b = PasswordManager.ChangePassword(Username, UpdatedUser.NewPasswordOne);

                if (b)
                {
                    ChangePasswordErrorMessage = "Your password was successfully changed!";

                    if(IndexModel.Administrator)
                    {
                        return RedirectToPage("UserManagement");
                    }
                    else
                    {
                        return RedirectToPage("StudentAdvising");
                    }
                }
                else
                {
                    ChangePasswordErrorMessage = "An error occurred while trying to change your password.";
                    return Page();
                }
            }
            else
            {
                ChangePasswordErrorMessage = "You did not enter the correct current password.";
                return Page();
            }
        }

        /// <summary></summary>
        public class ChangPasswordModel
        {
            /// <summary>Username</summary>
            [Required]
            public string OldPassword { get; set; }

            /// <summary>Password 1</summary>
            [Required]
            public string NewPasswordOne { get; set; }

            /// <summary>Password 2</summary>
            [Required]
            public string NewPasswordTwo { get; set; }
        }

        /// <summary>Error message for Create User form</summary>
        public static string ChangePasswordErrorMessage { get; set; } = "";

        /// <summary>Binds the LoginModel to a Login object for POST</summary>
        [BindProperty]
        public ChangPasswordModel UpdatedUser { get; set; }
    }
}
