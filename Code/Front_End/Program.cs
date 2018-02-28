using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CwuAdvising
{
    /// <summary>Entry point of the application</summary>
    public class Program
    {
        /// <summary>Location of the database configuration file</summary>
        private static string ConfigPath { get; set; } = "/var/aspnetcore/publish/Configuration.ini";

        /// <summary>Interface for accessing the database</summary>
        public static DatabaseInterface Database { get; set; } = null;

        /// <summary>Main method</summary>
        /// <param name="args">Arugments</param>
        public static void Main(string[] args)
        {
            Database = new DatabaseInterface(ConfigPath);
            BuildWebHost(args).Run();            
        }

        /// <summary></summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
