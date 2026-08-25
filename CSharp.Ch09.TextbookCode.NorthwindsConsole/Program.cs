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
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using NorthwindsConsole.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace NorthwindsConsole
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * This project's original textbook download used an EDMX-based, Database First EF6
         *   model (a .edmx file plus ~35 T4-generated entity/view/stored-procedure classes,
         *   covering the full Northwind schema). This port is adapted, not a byte-for-byte
         *   copy, see README.md and LectureNotes.md for the full reasoning: the demonstrated
         *   code here only ever touches Categories, Products, and the CustOrderHist stored
         *   procedure, so the model was simplified down to just those, built the same
         *   Code-First-against-an-existing-database way as
         *   CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework, rather than porting ~35
         *   generated files' worth of unused surface area.
         *
         * The five operations below (Select, Select with a join, Add, Update, Delete, and
         *   calling a stored procedure) match the original download's five operations
         *   exactly in intent, this project needs the "Northwinds" database set up first,
         *   see README.md.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Simple Select
                SimpleSelect();
                GenericFunctions.Pause();

                // Select with a join
                SelectWithJoin();
                GenericFunctions.Pause();

                // Add a record
                AddRecord();
                GenericFunctions.Pause();

                // Update a record
                UpdateRecord();
                GenericFunctions.Pause();

                // Delete a record
                DeleteRecord();
                GenericFunctions.Pause();

                // Call a stored procedure
                CallStoredProcedure();
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
        private static void SimpleSelect()
        {
            using var db = new NorthwindsEntities();

            var categories = from c in db.Categories
                              select c;

            Console.WriteLine("Categories:");
            foreach (Category category in categories)
            {
                Console.WriteLine($" - CategoryID: {category.CategoryID}, CategoryName: {category.CategoryName}");
            }
        }

        // Select with a join
        private static void SelectWithJoin()
        {
            using var db = new NorthwindsEntities();

            // .Include(p => p.Category) eager-loads the Category navigation property as
            //   part of this same query, rather than lazy-loading it later. Without this,
            //   reading product.Category inside the foreach loop below would trigger a
            //   NEW query on the same connection while this query's own DataReader is
            //   still open enumerating "products", throwing InvalidOperationException
            //   ("There is already an open DataReader..."). .ToList() also materializes
            //   the results up front, closing this query's reader before the loop runs
            //   at all, belt-and-suspenders alongside the eager load.
            var products = (from c in db.Categories
                            join p in db.Products.Include(p => p.Category) on c.CategoryID equals p.CategoryID
                            select p).ToList();

            Console.WriteLine("Products, joined to their Category:");
            foreach (Product product in products)
            {
                Console.WriteLine($" - ProductName: {product.ProductName}, CategoryName: {product.Category.CategoryName}");
            }
        }

        // Add a record
        private static void AddRecord()
        {
            using var db = new NorthwindsEntities();

            var category = new Category
            {
                CategoryName = "Alcohol",
                Description = "Happy Beverages"
            };

            db.Categories.Add(category);
            db.SaveChanges();

            Console.WriteLine($"Added Category '{category.CategoryName}' with CategoryID {category.CategoryID}");
        }

        // Update a record
        private static void UpdateRecord()
        {
            using var db = new NorthwindsEntities();

            var category = db.Categories.First(c => c.CategoryName == "Alcohol");
            category.Description = "Happy People";
            db.SaveChanges();

            Console.WriteLine($"Updated Category '{category.CategoryName}': Description is now '{category.Description}'");
        }

        // Delete a record
        private static void DeleteRecord()
        {
            using var db = new NorthwindsEntities();

            var category = db.Categories.First(c => c.CategoryName == "Alcohol");
            db.Categories.Remove(category);
            db.SaveChanges();

            Console.WriteLine($"Deleted Category '{category.CategoryName}'");
        }

        // Call a stored procedure
        private static void CallStoredProcedure()
        {
            using var db = new NorthwindsEntities();

            // The original EDMX-generated context exposed this as a strongly-typed
            //   db.CustOrderHist("ALFKI") method (an EDMX function import). The simplified
            //   Code First model here doesn't have function imports to generate, so this
            //   calls the same stored procedure the same way
            //   CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework's EfCallStoredProcedure()
            //   does, via Database.SqlQuery<T>().
            var custOrderHist = db.Database.SqlQuery<CustOrderHistResult>(
                "EXEC CustOrderHist @CustomerID", new SqlParameter("@CustomerID", "ALFKI")).ToList();

            Console.WriteLine("CustOrderHist('ALFKI'):");
            foreach (var result in custOrderHist)
            {
                Console.WriteLine($" - ProductName: {result.ProductName}, Total: {result.Total}");
            }
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
