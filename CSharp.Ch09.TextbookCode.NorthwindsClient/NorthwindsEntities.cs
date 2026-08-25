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
using System.Data.Services.Common;
using NorthwindsClient.Models;
#endregion

namespace NorthwindsClient
{
    /// <summary>
    /// A hand-written OData client context, standing in for the "Service References\
    /// NorthwindsServiceReference\Reference.cs" file the original download's Visual Studio
    /// "Add Service Reference" wizard would have generated automatically. Not tooling
    /// generated, written to mirror the shape that tooling produces closely enough that
    /// the CRUD calls in Program.cs work exactly the way they would against the real
    /// generated proxy: DataServiceContext as the base, one DataServiceQuery&lt;T&gt;
    /// property per entity set exposed by the service.
    /// </summary>
    public class NorthwindsEntities : DataServiceContext
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the NorthwindsEntities class, pointed at a running
        /// CSharp.Ch09.TextbookCode.NorthwindsWCFDataService instance
        /// </summary>
        /// <param name="serviceRoot">Root URI of the OData service</param>
        public NorthwindsEntities(Uri serviceRoot) : base(serviceRoot, DataServiceProtocolVersion.V3)
        {
        }
        #endregion

        #region Entity Sets
        /// <summary>
        /// Query root for the service's "Categories" entity set
        /// </summary>
        public DataServiceQuery<Category> Categories => CreateQuery<Category>("Categories");
        #endregion

        #region Public Methods
        /// <summary>
        /// Stage a new Category to be inserted on the next SaveChanges() call
        /// </summary>
        public void AddToCategories(Category category)
        {
            AddObject("Categories", category);
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
