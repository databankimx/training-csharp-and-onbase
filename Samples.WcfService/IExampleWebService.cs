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

#region Diretives
using System.ServiceModel;
using System.ServiceModel.Web;
using Samples.WcfService.Models.Objects;
#endregion

namespace Samples.WcfService
{
    /// <summary>
    /// Defines the interface (WSDL contract) for the web service
    /// </summary>
    [ServiceContract]
    public interface IExampleWebService
    {
        #region Interface Mathods
        // Verify that the web service is online
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Ping", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        // Verify that the web service is online and can accept incoming data
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/TestService", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ServiceTestResponse TestService(ServiceTestRequest request);

        // Verify that the web service is online and can accept incoming data (REST URI)
        [OperationContract]
        [WebGet(UriTemplate = "/TestServiceRest/{requestId}/{data}")]
        string TestServiceRest(string requestId, string data);

        // Look up location details by Zip code
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/LookupLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        LocationLookupResponse LookupLocation(LocationLookupRequest request);

        // Look up location details by Zip code (REST URI)
        [OperationContract]
        [WebGet(UriTemplate = "/LookupLocationRest/{requestId}/{zipCode}")]
        string LookupLocationRest(string requestId, string zipCode);
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
