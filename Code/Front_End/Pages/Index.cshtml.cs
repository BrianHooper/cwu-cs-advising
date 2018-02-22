using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CwuAdvising.Pages
{

    public class IndexModel : PageModel
    {
        public class StudentInfoModel
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string Quarter { get; set; }
            public string Year { get; set; }
            public string Degree { get; set; }
        }

        [BindProperty]
        public StudentInfoModel Index { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            DemoController.StudentName = Index.Name;
            DemoController.StudentID = Index.ID;
            DemoController.StudentQuarter = Index.Quarter;
            DemoController.StudentYear = Index.Year;
            DemoController.StudentDegree = Index.Degree;


            if (!ModelState.IsValid)
            {
                return Page();
            }

            // create and send the mail here
            return RedirectToPage("Advising");
        }
        public void OnGet()
        {

        }
    }
}
