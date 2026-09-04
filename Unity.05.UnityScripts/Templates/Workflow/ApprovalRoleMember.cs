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
using Hyland.Unity;
#endregion

namespace Unity._05.UnityScripts.Templates.Workflow
{
    /// <summary>
    /// Template workflow approval role member script.
    /// </summary>
    public class ApprovalRoleMember : IWorkflowApprovalRoleMemberScript
    {
        /// <summary>
        /// Returns the members of an approval role.
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        /// <param name="args">Approval arguments</param>
        /// <returns>The users who are members of the approval role.</returns>
        public IEnumerable<User> GetApprovalRoleMembers(Application app, WorkflowApprovalArgs args)
        {
            throw new NotImplementedException();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
