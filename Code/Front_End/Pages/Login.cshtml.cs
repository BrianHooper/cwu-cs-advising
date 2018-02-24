using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CwuAdvising.Pages
{
    public class LoginModel : PageModel
    {
        public class CredentialsContainer
        {
            private string username;
            private string password;

            public string Username
            {
                set => username = string.Copy(value);
            }
            public string Password
            {
                set => password = string.Copy(value);
            }
        }

        [BindProperty]
        protected CredentialsContainer Login { get; set; }

        public void OnGet()
        {
            
        }
    }
}
