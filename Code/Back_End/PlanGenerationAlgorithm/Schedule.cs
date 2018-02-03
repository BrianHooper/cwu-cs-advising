using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Quarter quarterName = new Quarter(2018, Season.Fall);
        static private uint ui_numberCredits = 5;
        public List<Course> courses1;
        public Schedule NextQuarter;
        Schedule nextquarter1;
        //HashSet<Course> h = new HashSet<Course>();
        public Schedule(Quarter quarter, uint ui_numberCredits)
        {
            quarter = new Quarter(3, Season.Fall);
            ui_numberCredits = 5;
            courses1 = new List<Course>();
            NextQuarter = nextquarter1;
        } // end Constructor

        public List<Course> addClass(Course c) {
            courses1.Add(c);
            return courses1;
        }
        public List<Course> removeClass(Course c)
        {
            courses1.Remove(c);
            return courses1;
        }
      
    }
}
