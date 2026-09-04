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
using Hyland.Unity;
using Hyland.Unity.EnterpriseIntegrationServer;
#endregion

namespace Unity._05.UnityScripts.Templates.EnterpriseIntegrationServer
{
    /// <summary>
    /// Template Enterprise Integration Server script, message broker response handler.
    /// </summary>
    public class EisBrokerResponseHandler : IEISMessageBrokerResponseHandlerScript
    {
        /// <summary>
        /// Called to handle a message broker response.
        /// </summary>
        /// <param name="unityApplication">Unity API Application Object</param>
        /// <param name="args">Event arguments</param>
        public void HandleMessageResponse(Application unityApplication, EISMessageMessageBrokerResponseHandlerArgs args)
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
