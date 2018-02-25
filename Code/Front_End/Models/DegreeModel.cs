using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising.Models
{
    /// <summary>Model for degree management page</summary>
    public class DegreeModel
    {

        /// <summary>Constructor for Degree</summary>
        /// <param name="name">Name of the degree</param>
        /// <param name="year">Academic year</param>
        public DegreeModel(string name, uint year)
        {
            this.name = name;
            this.year = year;
            this.requirements = new List<string>();
        }

        /// <summary>Name of the degree</summary>
        public string name { get; set; }

        /// <summary>Academic year for the degree</summary>
        public uint year { get; set; }

        /// <summary>List of course IDs required for the degree</summary>
        public List<string> requirements { get; set; }
    }
}
