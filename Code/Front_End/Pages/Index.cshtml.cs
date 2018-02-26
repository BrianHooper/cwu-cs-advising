using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CwuAdvising.Pages
{
    /// <summary>Model for index page</summary>
    public class IndexModel : PageModel
    {
        /// <summary>Public interface for accessing the database</summary>
        public static DatabaseInterface dbInterface = null;

        /// <summary>Attempts to connect to the database</summary>
        /// <returns>True if the connection was successful</returns>
        public static bool ConnectToDatabase()
        {
            dbInterface = new DatabaseInterface("/var/aspnetcore/publish/Configuration.ini");

            return dbInterface.connected;
        }
    }
}
