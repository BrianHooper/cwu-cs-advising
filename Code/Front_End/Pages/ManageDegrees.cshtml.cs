﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CwuAdvising.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Database_Object_Classes;

namespace CwuAdvising.Pages
{
    /// <summary>Model for degree management</summary>
    public class ManageDegreesModel : PageModel
    {
        /// <summary>Master list of Catalogs, identified by catalog year</summary>
        public static List<CatalogRequirements> CatalogList = new List<CatalogRequirements>();

        /// <summary>Master list of Degree Models</summary>
        public static List<DegreeModel> ModelList = new List<DegreeModel>();


        /// <summary>Retrieves the Catalog list from the database</summary>
        /// <returns>True if the database query was successful</returns>
        public static bool GetCatalogFromDatabase()
        {
            CatalogList = new List<CatalogRequirements>();
            //Program.Database.RetrieveRecord(???, Database_Handler.OperandType.CatalogRequirements);
            TESTBuildDegreeList();
            return true;
        }

        /// <summary>TEST build degree list</summary>
        public static void TESTBuildDegreeList()
        {
            List<Course> empty = new List<Course>();
            CatalogCreditRequirements emptyCR = new CatalogCreditRequirements(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            //2017 
            {
                // List of degrees
                List<DegreeRequirements> DegreeRequirements2017 = new List<DegreeRequirements>();

                // Definition for BS - Computer Science
                List<Course> CSRequirements = new List<Course>();
                Course CS301 = new Course("Data Structures I", "CS301", 4, false, new bool[] { false, true, false, true });
                List<Course> CS302Prereqs = new List<Course>
                {
                    CS301
                };
                Course CS302 = new Course("Data Structures II", "CS302", 4, false, new bool[] { true, false, false, true }, CS302Prereqs);
                CSRequirements.Add(CS301);
                CSRequirements.Add(CS302);
                DegreeRequirements computerscience = new DegreeRequirements(CSRequirements, empty, empty, empty, 0, 0.0, "BS - Computer Science");
                DegreeRequirements2017.Add(computerscience);

                // Definition for BS - Mathematics
                List<Course> MATHRequirements = new List<Course>();
                Course MATH260 = new Course("Sets and Logic", "MATH260", 5, false, new bool[] { true, true, false, true });
                List<Course> MATH260Prereqs = new List<Course>
                {
                    MATH260
                };
                Course MATH330 = new Course("Discrete Math", "MATH330", 5, false, new bool[] { false, true, false, true }, MATH260Prereqs);
                MATHRequirements.Add(MATH260);
                MATHRequirements.Add(MATH330);
                DegreeRequirements mathematics = new DegreeRequirements(MATHRequirements, empty, empty, empty, 0, 0.0, "BS - Mathematics");
                DegreeRequirements2017.Add(mathematics);
                
                CatalogRequirements Catalog2017 = new CatalogRequirements("2017", 0, 0.0, emptyCR, DegreeRequirements2017);
                CatalogList.Add(Catalog2017);
            }

            //2018
            {
                // List of degrees
                List<DegreeRequirements> DegreeRequirements2018 = new List<DegreeRequirements>();

                // Definition for BS - Computer Science
                List<Course> CSRequirements = new List<Course>();
                Course CS311 = new Course("Computer Architecture I", "CS311", 4, false, new bool[] { false, true, false, true });
                List<Course> CS312Prereqs = new List<Course>
                {
                    CS311
                };
                Course CS312 = new Course("Computer Architecture II", "CS312", 4, false, new bool[] { true, false, false, true }, CS312Prereqs);
                CSRequirements.Add(CS311);
                CSRequirements.Add(CS312);
                DegreeRequirements computerscience = new DegreeRequirements(CSRequirements, empty, empty, empty, 0, 0.0, "BS - Computer Science");
                DegreeRequirements2018.Add(computerscience);

                // Definition for BS - Mathematics
                List<Course> MATHRequirements = new List<Course>();
                Course MATH260 = new Course("Sets and Logic", "MATH260", 5, false, new bool[] { true, true, false, true });
                List<Course> MATH260Prereqs = new List<Course>
                {
                    MATH260
                };
                Course MATH330 = new Course("Discrete Math", "MATH330", 5, false, new bool[] { false, true, false, true }, MATH260Prereqs);
                MATHRequirements.Add(MATH260);
                MATHRequirements.Add(MATH330);
                DegreeRequirements mathematics = new DegreeRequirements(MATHRequirements, empty, empty, empty, 0, 0.0, "BS - Mathematics");
                DegreeRequirements2018.Add(mathematics);

                CatalogRequirements Catalog2018 = new CatalogRequirements("2018", 0, 0.0, emptyCR, DegreeRequirements2018);
                CatalogList.Add(Catalog2018);
            }
        }

        /// <summary>Converts a Database DegreeRequirement to a front-end DegreeModel</summary>
        /// <param name="year">Catalog Year</param>
        /// <param name="degree">DegreeRequirements database object</param>
        /// <returns>DegreeModel representing the degree</returns>
        public static DegreeModel RequirementsToDegreeModel(string year,  DegreeRequirements degree)
        {
            DegreeModel model = new DegreeModel(degree.Name, UInt32.Parse(year));

            foreach(Course course in degree.GeneralRequirements)
            {
                model.requirements.Add(course.ID);
            }

            return model;
        }


        /// <summary>Converts a DegreeModel object to a DegreeRequirements Object</summary>
        /// <param name="Degree">DegreeModel to be parsed</param>
        /// <returns>DegreeRequirements object corresponding to the given DegreeModel</returns>
        public static DegreeRequirements DegreeModelToRequirements(DegreeModel Degree)
        {
            List<Course> DegreeCourseList = new List<Course>();

            ManageCoursesModel.GetCoursesFromDatabase();

            foreach(string CourseID in Degree.requirements)
            {
                try
                {
                    Course MatchingCourse = ManageCoursesModel.MasterCourseList.Find(delegate (Course C) { return C.ID == CourseID; });
                    DegreeCourseList.Add(MatchingCourse);
                }
                catch (ArgumentException)
                {

                }
            }

            List<Course> EmptyList = new List<Course>();
            return new DegreeRequirements(DegreeCourseList, EmptyList, EmptyList, EmptyList, 0, 0.0, Degree.name);
        }

        /// <summary>Loads the master catalog list into a list of DegreeModels</summary>
        public static void LoadDegreeModelList()
        {
            GetCatalogFromDatabase();
            ModelList = new List<DegreeModel>();
            foreach (CatalogRequirements catalog in CatalogList)
            {
                foreach (DegreeRequirements requirement in catalog.DegreeRequirements)
                {
                    ModelList.Add(RequirementsToDegreeModel(catalog.ID, requirement));
                }
            }
        }

        /// <summary>Parses the degree list</summary>
        /// <returns>DegreeModel list as a JSON string</returns>
        public static string ReadModel()
        {
            if(ModelList != null)
            {
                return JsonConvert.SerializeObject(ModelList);
            }
            else
            {
                return "";
            }
        }

        /// <summary>Updates database with the modified degree</summary>
        /// <param name="model">Model of the degree to be updated</param>
        /// <returns>true if the update was successful</returns>
        public static void WriteDatabaseDegree(DegreeModel model)
        {
            // Convert the DegreeModel to DegreeRequirements
            string CatalogYear = model.year.ToString();
            DegreeRequirements Degree = DegreeModelToRequirements(model);
            CatalogRequirements Catalog;

            // Create a new list of DegreeRequirements and add the modified degree
            List<DegreeRequirements> NewDegreeRequirements = new List<DegreeRequirements>
            {
                Degree
            };
            
            try
            {
                // Find a catalog matching the catalog year of the modified degree
                Catalog = CatalogList.Find(delegate (CatalogRequirements CR) { return CR.ID == CatalogYear; });
                
                // Add any degrees in the catalog other than the modifed degree
                foreach(DegreeRequirements OldDegree in Catalog.DegreeRequirements)
                {
                    if(OldDegree.Name != Degree.Name)
                    {
                        NewDegreeRequirements.Add(OldDegree);
                    }
                }

            }
            catch(ArgumentNullException)
            {
                
            }

            // Create a new CatalogRequirement with updated DegreeRequirements
            CatalogCreditRequirements EmptyCredits = new CatalogCreditRequirements();
            Catalog = new CatalogRequirements(CatalogYear, 0, 0.0, EmptyCredits, NewDegreeRequirements);

            // Update the database
            Program.Database.UpdateRecord(Catalog, Database_Handler.OperandType.CatalogRequirements);
        }

        /// <summary>Retrieves a modified degree as JSON data from POST</summary>
        /// <returns>JsonResult containing success/error status</returns>
        public ActionResult OnPostSendDegree()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                {
                    var ModifiedDegree = JsonConvert.DeserializeObject<DegreeModel>(requestBody);

                    if(Program.Database.connected)
                    {
                        WriteDatabaseDegree(ModifiedDegree);
                    }
                    
                    return new JsonResult("Courses saved succesfully.");
                }
                else
                {
                    return new JsonResult("Error passing data to server.");
                }
            }
        }
    }
}