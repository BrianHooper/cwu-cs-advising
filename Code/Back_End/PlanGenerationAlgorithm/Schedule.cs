using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Schedule
    {
        public Quarter quarterName = new Quarter(2018, Season.Fall);
        private static uint ui_numberCredits = 0;
        public List<Course> courses;
        public Schedule NextQuarter, previousQuarter;
        //HashSet<Course> h = new HashSet<Course>();
        public Schedule(Quarter quarter, uint ui_numberCredits)
        {
            quarter = new Quarter(3, Season.Fall);
            ui_numberCredits = 5;
            courses = new List<Course>();
        } // end Constructor


        public List<Course> addClass(Course c) {
            courses.Add(c);
            return courses;
        }
        public Schedule nextQuarter()
        {

            if (NextQuarter == null)
            {
                //quarterName = this.quarterName++;
                NextQuarter = new Schedule(quarterName, 0);
                NextQuarter.previousQuarter = this;
            }
            
               // NextQuarter.quarterName = this.quarterName++;
            
            //this.quarterName++;
            return NextQuarter;
            
        }


        public List<Course> removeClass(Course c)
        {
            courses.Remove(c);
            return courses;
        }
      
    }
}
