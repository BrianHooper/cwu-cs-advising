﻿using System;
using System.Collections.Generic;
using Database_Object_Classes;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising.Models
{
    public class CourseModel
    {
        public string Title { get; set; }
        public string Department { get; set; } // Mathematics or Computer Science
        public string Number { get; set; }
        public string Credits { get; set; }
        public string Offered { get; set; }
        public bool RequiresMajor { get; set; }
        public List<CourseModel> PreReqs { get; set; } = new List<CourseModel>();

        /// <summary>Explicit cast operator for CourseModel to Course conversion.</summary>
        /// <returns>A Course object equivalent to the given CourseModel.</returns>
        public static explicit operator Course(CourseModel model)
        {
            List<Course> preRequs = new List<Course>();

            bool[] offered = new bool[4];

            foreach(CourseModel m in model.PreReqs)
            {
                preRequs.Add((Course)m);
            } // end foreach

            for (int i = 0; i < model.Offered.Length; i++)
            {
                switch(model.Offered[i])
                {
                    case '1':
                        offered[0] = true;
                        break;
                    case '2':
                        offered[1] = true;
                        break;
                    case '3':
                        offered[2] = true;
                        break;
                    case '4':
                        offered[3] = true;
                        break;
                } // end switch
            } // end for

            string ID = string.Empty;

            switch(model.Department)
            {
                case "Computer Science":
                    ID = "CS";
                    break;
                case "Mathematics":
                    ID = "Math";
                    break;
            } // end switch
            
            return new Course(model.Title, (string.Concat(ID, model.Number)), uint.Parse(model.Credits), model.RequiresMajor, offered, preRequs);
        } // end explicit cast operator
    }
}