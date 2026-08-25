#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using NorthwindsClient.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace NorthwindsClient
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * This project needs CSharp.Ch09.TextbookCode.NorthwindsWCFDataService actually
         *   running first (Visual Studio only, see that project's LectureNotes.md), on
         *   http://localhost:8999/ (the port the original download's own .csproj already
         *   configured). If that's not running, every method below will throw a connection
         *   error, that's expected, not a bug in this project.
         *
         * This is an ADAPTED port, not byte-for-byte preserved, see LectureNotes.md for the
         *   full reasoning: the original download's active code created a "NorthwindsEntities"
         *   client (from a Visual-Studio-generated Service Reference this migration doesn't
         *   port), then left every actual USE of it commented out, only the raw JSON request
         *   at the bottom of the file was genuinely active. This version hand-writes a small
         *   OData client context (see NorthwindsEntities.cs) so all five originally-demonstrated
         *   operations (Select, Add, Update, Delete, and the raw JSON request) actually run.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                var db = new NorthwindsEntities(new Uri("http://localhost:8999/NorthwindsService.svc/"));

                #region Chapter Lessons
                // Simple Select
                SimpleSelect(db);
                GenericFunctions.Pause();

                // Add a record
                AddRecord(db);
                GenericFunctions.Pause();

                // Update a record
                UpdateRecord(db);
                GenericFunctions.Pause();

                // Delete a record
                DeleteRecord(db);
                GenericFunctions.Pause();

                // Request data as JSON using a raw client request
                RequestDataAsJson();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Simple Select
        private static void SimpleSelect(NorthwindsEntities db)
        {
            var categories = from c in db.Categories
                              select c;

            Console.WriteLine("Categories (via the OData client):");
            foreach (Category category in categories)
            {
                Console.WriteLine($" - CategoryId: {category.CategoryID}, CategoryName: {category.CategoryName}");
            }
        }

        // Add a record
        private static void AddRecord(NorthwindsEntities db)
        {
            var category = new Category
            {
                CategoryName = "Alcohol",
                Description = "Happy Beverages"
            };

            db.AddToCategories(category);
            DataServiceResponse response = db.SaveChanges();

            if (response.First().StatusCode == (int)HttpStatusCode.Created)
            {
                Console.WriteLine($"New CategoryId: {category.CategoryID}");
            }
            else
            {
                Console.WriteLine($"Error: {response.First().Error.Message}");
            }
        }

        // Update a record
        private static void UpdateRecord(NorthwindsEntities db)
        {
            var category = db.Categories.Where(c => c.CategoryName == "Alcohol").FirstOrDefault();

            if (category == null)
            {
                Console.WriteLine("\"Alcohol\" category not found, run AddRecord() first.");
                return;
            }

            category.Description = "Happy People";

            db.UpdateObject(category);
            db.SaveChanges();

            Console.WriteLine($"Updated Category '{category.CategoryName}': Description is now '{category.Description}'");
        }

        // Delete a record
        private static void DeleteRecord(NorthwindsEntities db)
        {
            var category = db.Categories.Where(c => c.CategoryName == "Alcohol").FirstOrDefault();

            if (category == null)
            {
                Console.WriteLine("\"Alcohol\" category not found, nothing to delete.");
                return;
            }

            db.DeleteObject(category);
            db.SaveChanges();

            Console.WriteLine($"Deleted Category '{category.CategoryName}'");
        }

        // Request data as JSON using a raw client request
        private static void RequestDataAsJson()
        {
            var req = (HttpWebRequest)WebRequest.Create("http://localhost:8999/NorthwindsService.svc/Categories(1)?$select=CategoryID,CategoryName,Description");
            req.Accept = "application/json;odata=verbose";

            using var resp = (HttpWebResponse)req.GetResponse();
            using var readStream = new StreamReader(resp.GetResponseStream() ?? Stream.Null);

            string jsonString = readStream.ReadToEnd();
            Console.WriteLine("Raw JSON response for Categories(1):");
            Console.WriteLine(jsonString);
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
