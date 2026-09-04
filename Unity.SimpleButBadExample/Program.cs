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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hyland.Unity;
using Hyland.Unity.Extensions;
#endregion

#region Warning Suppression
#pragma warning disable S5332   // Suppress warnings for hard-coded URLs
#pragma warning disable S5443   // Suppress warnings for publicly writable directories
#pragma warning disable S1075   // Suppress warnings for hard-coded file paths
#pragma warning disable S112    // Suppress warnings for generic exception types
#pragma warning disable IDE0270 // Suppress suggestions to use coalesce expression
#pragma warning disable IDE0305 // Suppress suggestions to simplify collection initialization
#pragma warning disable IDE0028 // Suppress suggestions to simplify collection initialization
#endregion

namespace Unity.SimpleButBadExample
{
    #region Training Notes
    /*
     * *Migration Note: this class is deliberately left BAD. Every one of the following is
     * left exactly as it was, on purpose, see LectureNotes.md for the full teaching
     * discussion of each:
     *
     * - Hardcoded credentials (AppServer, UserName, Password) as compile-time constants,
     *   right there in source control.
     * - One monolithic Main() method doing connect, query, retrieve, update, upload, and
     *   delete, with no separation of concerns at all.
     * - Plain ApplicationException instead of DatabankException, the standard this entire
     *   training set otherwise uses everywhere.
     * - No dependency injection, no configuration file, no testability whatsoever, this
     *   whole class cannot be unit tested without a live OnBase connection.
     *
     * Compare this directly against Unity.00.CommonFunctionality/Unity.01.ConnectingToOnBase,
     * which do all of this correctly.
     */
    #endregion

    /// <summary>
    /// Example Unity API Programming
    /// </summary>
    internal static class Program
    {
        #region Test Constants
        // OnBase Connection Values
        private const string AppServer = "http://OnBaseSandboxVM/AppServer/Service.asmx";
        private const string DataSource = "OnBaseSandbox";
        private const string UserName = "MANAGER";
        private const string Password = "password";

        // Retrieval and Storage Values
        private const string DocTypeName = "CON - Primary Document";
        private const string KeywordName = "Description";
        private const string KeywordValue = "DEMO";
        private const string NewKeywordName = "File Type";
        private const string NewKeywordValue = "API";

        // Temp File Values
        private const string TempPath = @"C:\Temp";
        private const string TempFile = "Sample.pdf";
        #endregion

        #region Private Globals
        // Unity API Application Object
        private static Application app; // Unity.Application should NEVER be static in a real application

        // List to hold retrieved documents
        private static List<Document> documents;

        // Stores new OnBase document
        private static Document document;
        #endregion

        #region Main Executable Method
        // Main Executable Method
        private static void Main()
        {
            try
            {
                Connect();
                Pause();

                RetrieveDocuments();
                Pause();

                if (documents.Count > 0)
                {
                    var doc = documents[0];

                    ReportDocumentDetails(doc);
                    Pause();

                    GetDocumentFile(doc);
                    Pause();

                    UpdateDocumentKeyword(doc);
                    Pause();

                    UploadDocumentRevision(doc);
                    Pause();
                }

                UploadNewDocument();
                Pause();

                DeleteDocument(document);
                Pause();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                Disconnect();
                Console.WriteLine("Done! Press <ENTER> to exit...");
                Console.ReadLine();
            }
        }
        #endregion

        #region Helper Functions
        // Connect to OnBase
        private static void Connect()
        {
            try
            {
                var authProps =
                    Application.CreateOnBaseAuthenticationProperties(AppServer, UserName, Password, DataSource);
                app = Application.Connect(authProps);
                Console.WriteLine($"Connected to OnBase session [{app.SessionID}]...");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error connecting to OnBase!", ex);
            }
        }

        // Retrieve a list of documents
        private static void RetrieveDocuments()
        {
            try
            {
                var docType = app.Core.DocumentTypes.Find(DocTypeName);
                if (docType == null)
                    throw new ApplicationException($"Cannot find document type [{DocTypeName}]!");

                var keyType = app.Core.KeywordTypes.Find(KeywordName);
                if (keyType == null)
                    throw new ApplicationException($"Cannot find keyword type [{KeywordName}]!");

                if (!keyType.TryCreateKeyword(KeywordValue, out var keyword))
                    throw new ApplicationException($"Cannot create keyword [{KeywordName}] = [{KeywordValue}]!");

                var query = app.Core.CreateDocumentQuery();
                query.AddDocumentType(docType);
                query.AddKeyword(keyword);

                documents = query.Execute(100).ToList();
                Console.WriteLine($"Retrieved {documents.Count} document{(documents.Count == 1 ? "" : "s")}...");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving documents!", ex);
            }
        }

        // Explore document metadata
        private static void ReportDocumentDetails(Document doc)
        {
            try
            {
                Console.WriteLine($"Document Handle: {doc.ID}");
                Console.WriteLine($"Document Name:   {doc.Name}");
                Console.WriteLine($"Document Date:   {doc.DocumentDate:d}");
                Console.WriteLine($"Archived Date:   {doc.DateStored:d}");
                Console.WriteLine($"Document Type:   {doc.DocumentType.Name}");
                foreach (var record in doc.KeywordRecords)
                {
                    Console.WriteLine(record.KeywordRecordType.RecordType == RecordType.StandAlone ? "Stand-Alone Keywords" : record.KeywordRecordType.Name);
                    foreach (var keyword in record.Keywords)
                    {
                        if (keyword == null || keyword.IsBlank) continue;
                        Console.WriteLine($" - [{keyword.KeywordType.Name}] = [{keyword.Value}]");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error reporting document details!", ex);
            }
        }

        // Download the file from a document
        private static void GetDocumentFile(Document doc)
        {
            try
            {
                var rendition = doc.DefaultRenditionOfLatestRevision;
                var pageData = app.Core.Retrieval.Native.GetDocument(rendition);
                string path = Path.Combine(TempPath, $"{Guid.NewGuid()}.{rendition.FileExtension}");
                Utility.WriteStreamToFile(pageData.Stream, path);
                if (!File.Exists(path))
                    throw new ApplicationException($"Failed to create file [{path}]!");
                Console.WriteLine($"Created temp file [{path}]...");
                Process.Start(path);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error getting document file!", ex);
            }
        }

        // Modify document metadata
        private static void UpdateDocumentKeyword(Document doc)
        {
            try
            {
                var mod = doc.CreateKeywordModifier();

                var keyType = app.Core.KeywordTypes.Find(NewKeywordName);
                if (keyType == null)
                    throw new ApplicationException($"Cannot find keyword type [{NewKeywordName}]!");

                if (!keyType.TryCreateKeyword(NewKeywordValue, out var newKeyword))
                    throw new ApplicationException($"Failed to create keyword [{NewKeywordName}] = [{NewKeywordValue}]!");

                Keyword oldKeyword = null;
                var record = doc.KeywordRecords.Find(keyType);
                if (record != null) oldKeyword = record.Keywords.Find(keyType);

                if (oldKeyword == null)
                {
                    mod.AddKeyword(newKeyword);
                }
                else
                {
                    mod.UpdateKeyword(oldKeyword, newKeyword);
                }

                var docLock = doc.LockDocument();
                if (docLock.Status != DocumentLockStatus.LockObtained)
                    throw new ApplicationException($"Failed to lock document [{doc.ID}]!");

                mod.ApplyChanges();
                docLock.Release();

                Console.WriteLine("Updated document...");
                Pause();
                ReportDocumentDetails(doc);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating document keyword!", ex);
            }
        }

        // Upload a new revision to a document
        private static void UploadDocumentRevision(Document doc)
        {
            try
            {
                var pdfFileType = app.Core.FileTypes.Find(16);
                var revisionProps = app.Core.Storage.CreateStoreRevisionProperties(doc, pdfFileType);
                var newDocRev = app.Core.Storage.StoreNewRevision(new List<string> {Path.Combine(TempPath, TempFile)}, revisionProps);
                Console.WriteLine($"Uploaded new revision to document [{newDocRev.ID}]");
                Pause();
                GetDocumentFile(newDocRev);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error uploading document revision!", ex);
            }
        }

        // Upload a new document
        private static void UploadNewDocument()
        {
            try
            {
                var docType = app.Core.DocumentTypes.Find(DocTypeName);
                if (docType == null)
                    throw new ApplicationException($"Cannot find document type [{DocTypeName}]!");
                
                var pdfFileType = app.Core.FileTypes.Find(16);
                var newDocProps = app.Core.Storage.CreateStoreNewDocumentProperties(docType, pdfFileType);

                var keyType = app.Core.KeywordTypes.Find(KeywordName);
                if (keyType == null)
                    throw new ApplicationException($"Cannot find keyword type [{KeywordName}]!");

                if (!keyType.TryCreateKeyword(KeywordValue, out var keyword))
                    throw new ApplicationException($"Cannot create keyword [{KeywordName}] = [{KeywordValue}]!");

                newDocProps.AddKeyword(keyword);

                var newKeyType = app.Core.KeywordTypes.Find(NewKeywordName);
                if (newKeyType == null)
                    throw new ApplicationException($"Cannot find keyword type [{NewKeywordName}]!");

                if (!newKeyType.TryCreateKeyword(NewKeywordValue, out var newKeyword))
                    throw new ApplicationException($"Cannot create keyword [{NewKeywordName}] = [{NewKeywordValue}]!");

                newDocProps.AddKeyword(newKeyword);

                document = app.Core.Storage.StoreNewDocument(new List<string>{Path.Combine(TempPath, TempFile)}, newDocProps);
                Console.WriteLine($"Uploaded new document [{document.ID}]");
                Pause();

                ReportDocumentDetails(document);
                GetDocumentFile(document);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error uploading new document!", ex);
            }
        }

        // Delete an existing document
        private static void DeleteDocument(Document doc)
        {
            try
            {
                long id = doc.ID;
                app.Core.Storage.DeleteDocument(doc);
                Console.WriteLine($"Deleted document [{id}]!");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error deleting document!", ex);
            }
        }

        // Disconnect from OnBase
        private static void Disconnect()
        {
            try
            {
                if (app == null || !app.IsConnected) return;
                app.Disconnect();
                app.Dispose();
                app = null;
                Console.WriteLine("Disconnected from OnBase...");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error disconnecting from OnBase!", ex);
            }
        }

        // Process and log exceptions
        private static void HandleException(Exception ex)
        {
            while (ex != null)
            {
                Console.WriteLine(ex);
                ex = ex.InnerException;
            }
        }

        // Wait for user interaction
        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to continue...");
            Console.ReadLine();
            Console.Clear();
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
