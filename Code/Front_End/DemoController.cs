using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CwuAdvising
{
    public partial class DemoController
    {

        public static String StudentName { get; set; } = "TestStudent";
        public static String StudentID { get; set; } = "1235";
        public static String StudentQuarter { get; set; } = "Spring";
        public static String StudentYear { get; set; } = "2020";
        public static String StudentDegree { get; set; } = "Example Degree";

        public static List<SelectListItem> GetQuarters()
        {
            List<SelectListItem> QuarterList = new List<SelectListItem>();

            SelectListItem Fall = new SelectListItem();
            Fall.Text = "Fall";
            Fall.Value = "Fall";
            QuarterList.Add(Fall);

            SelectListItem Winter = new SelectListItem();
            Winter.Text = "Winter";
            Winter.Value = "Winter";
            QuarterList.Add(Winter);

            SelectListItem Spring = new SelectListItem();
            Spring.Text = "Spring";
            Spring.Value = "Spring";
            QuarterList.Add(Spring);

            return QuarterList;
        }


        public static List<SelectListItem> GetDegrees()
        {
            List<SelectListItem> DegreeList = new List<SelectListItem>();

            SelectListItem none = new SelectListItem();
            none.Text = "-----";
            none.Value = "-----";
            DegreeList.Add(none);

            SelectListItem CS = new SelectListItem();
            CS.Text = "BS - Computer Science";
            CS.Value = "BS - Computer Science";
            DegreeList.Add(CS);

            SelectListItem IT = new SelectListItem();
            IT.Text = "BS - Information Technology";
            IT.Value = "BS - Information Technology";
            DegreeList.Add(IT);

            return DegreeList;
        }
        
    }
}
