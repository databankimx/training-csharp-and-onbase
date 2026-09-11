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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Configuration;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi._00.CommonFunctionality.Models.Objects;
using SysConfig = System.Configuration;
#endregion

namespace RestApi._01.ConnectingToOnBase.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: CORRECTION, this class was originally static (process-wide, one
     * shared session for the whole application), matching how RestApi.00's own
     * ServiceLocation documents Settings as an admin-style, global concept. That was
     * fine for RestApi.TestHarness (a single-user desktop app), but wrong for
     * RestApi.TestHarness.Web: every user of a web app needs their OWN IdP token and OWN
     * OnBase session, not a shared one where one user's disconnect (or session timeout)
     * silently breaks every other user's session. Converted to an ordinary instance
     * class specifically for that reason, an explicit, confirmed requirement, not
     * over-engineering.
     *
     * Two constructors, for the two different contexts this library now serves:
     *  - The parameterless constructor still auto-loads ServiceLocation/IdpSettings from
     *    the XML config file (App.config for RestApi.TestHarness), unchanged from the
     *    old static constructor's own behavior, still the right approach for a
     *    single-user desktop app with one shared settings file.
     *  - SessionManagement(ServiceLocation, IdpSettings) takes them explicitly, for
     *    RestApi.TestHarness.Web: one instance per user session (registered scoped in
     *    ASP.NET Core's DI container), built from shared appsettings.json-bound
     *    configuration (the server/IdP infrastructure settings) plus that specific
     *    user's own credentials (entered on the web app's own Connect page), not one
     *    shared instance for every visitor.
     *
     * Every other behavior described below is unchanged from the original static design,
     * just operating on instance fields instead of static ones:
     *
     * - GetAccessTokenAsync() obtains a Bearer token from the Hyland IdP (via
     *   IdpAuthentication), matching AuthenticationMode the same way Unity.01's
     *   ConnectNewSession() switches on it, only OnBaseCredentials is implemented, the
     *   other three throw NotImplementedException, see AuthenticationMode's own Training
     *   Notes for why each is stubbed.
     *
     * - ConnectAsync() then makes ONE authenticated request (GET document-type-groups, a
     *   lightweight, read-only taxonomy lookup, chosen the same way Unity.TestHarness's
     *   own "Test API Availability" ping is lightweight and harmless) to actually
     *   establish the OnBase session: per the Authentication guide, obtaining a token
     *   does NOT by itself create a session or consume a license, the FIRST authenticated
     *   request to the Document Management API does that, and returns a
     *   Cookie.Session.OnBase.Hyland cookie that must accompany every subsequent request.
     *
     * - That cookie is handled automatically via HttpClientHandler's own CookieContainer
     *   (UseCookies = true), not manual Set-Cookie header extraction/reattachment: once
     *   the handler receives it on the connect request, it's sent automatically on every
     *   later request through the same HttpClient, exactly like a browser would.
     *
     * - There is NO SessionId-reconnect equivalent here (see ServiceLocation's own
     *   Training Notes for why: the cookie IS the session, not a client-suppliable
     *   value), so unlike Unity.01's Connect(), there's no "try reconnecting first, fall
     *   back to a new session" branch, ConnectAsync() always establishes a brand new
     *   session.
     *
     * - KeepAlive drives an actual background heartbeat (System.Threading.Timer, firing
     *   every 4 minutes, just under the cookie's own 5-minute idle expiry per the
     *   Authentication guide), not a one-time connect-time flag the way Unity API's own
     *   IsDisconnectEnabled is. See StartHeartbeat/StopHeartbeat below.
     *
     * GetHttpClient() is the equivalent of handing out the Unity API's own Application
     * object: RestApi.02-04 call it to get a fully-configured (Bearer token + session
     * cookie) HttpClient for their own Document Management API calls, rather than each
     * managing authentication themselves. Since this class is no longer static, RestApi.02-04's
     * own helper classes can no longer fall back to a global "the" SessionManagement
     * instance when no HttpClient is explicitly supplied, see those projects' own
     * Training Notes for the corresponding change (an HttpClient is now a REQUIRED
     * constructor parameter there, not an optional one with a static fallback).
     *
     * GetFormsHttpClient() is the same idea for the separate Forms API (confirmed: every
     * OnBase REST API is served by the same server and honors the same Bearer token, see
     * ServiceLocation's own Training Notes). Its HttpClient shares the SAME underlying
     * HttpClientHandler/CookieContainer as the main one, rather than each having its own,
     * a deliberately defensive choice: whether the Forms API's session cookie is scoped
     * to be shared with the Document Management API's on this server isn't confirmed
     * either way, sharing the container means it works automatically if the server does
     * scope it that broadly, and costs nothing if it doesn't (each API's own first
     * request would still establish its own session independently in that case). Both
     * HttpClient instances are constructed with disposeHandler: false, so disposing
     * either one doesn't take the shared handler out from under the other; the handler
     * itself is disposed once, explicitly, in DisconnectAsync().
     *
     * This class also implements IDisposable now (it didn't need to as a static class,
     * nothing ever tore it down): ASP.NET Core's DI container disposes a scoped service
     * automatically at the end of each request/session lifetime, this is what actually
     * releases the underlying HttpClientHandler/HttpClient instances (and, via
     * DisconnectAsync's own disposal, everything it owns) rather than leaking them.
     * Dispose() does NOT call DisconnectAsync() itself (disposal must stay synchronous
     * and shouldn't make a final network call as a side effect of teardown), just
     * releases local resources; callers that want a clean server-side disconnect should
     * still call DisconnectAsync() explicitly beforehand (e.g. RestApi.TestHarness.Web's
     * own Connect page having a Disconnect button, or a request-ending piece of
     * middleware).
     */
    #endregion

    /// <summary>
    /// Manage, connect, and disconnect an OnBase session via the Document Management (REST) API.
    /// </summary>
    public class SessionManagement : IDisposable
    {
        #region Constants
        // How often to send a heartbeat while KeepAlive is true, comfortably under the
        // session cookie's own 5-minute idle expiry (see the Authentication guide's own
        // Heartbeat section)
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(4);
        #endregion

        #region Private Members
        // The configured HttpClientHandler, shared by both httpClient and
        // formsHttpClient (see this class's own Training Notes on GetFormsHttpClient for
        // why); recreated on every Connect, so a stale session cookie from a prior
        // connection is never accidentally reused
        private HttpClientHandler sharedHandler;

        // The configured HttpClientHandler's CookieContainer; recreated on every Connect,
        // so a stale session cookie from a prior connection is never accidentally reused
        #pragma warning disable S1450 // Keep as a field for future extensibility
        private CookieContainer cookieContainer;
        #pragma warning restore S1450

        // The HttpClient every Document Management API request (this project's own
        // connect/heartbeat/disconnect calls, and RestApi.02-04's own calls via
        // GetHttpClient()) goes through once connected
        private HttpClient httpClient;

        // The HttpClient every Forms API request goes through once connected, only built
        // when ServiceLocation.FormsApiUrl is configured (optional, see GetFormsHttpClient())
        private HttpClient formsHttpClient;

        // Fires the periodic heartbeat while KeepAlive is true and a session is active
        private Timer heartbeatTimer;

        // Set once Dispose() has run, guards against double-disposal
        private bool disposed;
        #endregion

        #region Properties
        /// <summary>
        /// OnBase API Server connection settings.
        /// </summary>
        public ServiceLocation ServiceLocation { get; set; }

        /// <summary>
        /// Hyland Identity Provider (IdP) Settings, used to obtain a Bearer token for
        /// whichever <see cref="ServiceLocation"/>'s AuthenticationMode is configured.
        /// </summary>
        public IdpSettings IdpSettings { get; set; }

        /// <summary>
        /// Whether a session is currently established.
        /// </summary>
        public bool IsConnected { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the SessionManagement class, loading
        /// <see cref="ServiceLocation"/>/<see cref="IdpSettings"/> from the XML config
        /// file (App.config), for a single-user desktop app with one shared settings
        /// file.
        /// </summary>
        public SessionManagement()
        {
            var settings = (RestApiSettings)SysConfig.ConfigurationManager.GetSection(RestApiSettings.SectionName);
            ServiceLocation = settings.ServiceLocation;
            IdpSettings = settings.IdpSettings;
        }

        /// <summary>
        /// Create a new instance of the SessionManagement class with explicitly-supplied
        /// settings, for a per-user (e.g. web) context where each user needs their own
        /// session, built from shared server/IdP configuration plus that user's own
        /// credentials, rather than one shared, process-wide settings file.
        /// </summary>
        /// <param name="serviceLocation">OnBase API Server connection settings.</param>
        /// <param name="idpSettings">Hyland Identity Provider (IdP) settings.</param>
        public SessionManagement(ServiceLocation serviceLocation, IdpSettings idpSettings)
        {
            ServiceLocation = serviceLocation;
            IdpSettings = idpSettings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects to OnBase: obtains a Bearer token from the Hyland IdP, then makes one
        /// authenticated request to the Document Management API to actually establish the
        /// session (see this class's own Training Notes for why that's a separate step
        /// from obtaining the token).
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                if (ServiceLocation == null) throw new DatabankException("ServiceLocation cannot be null!");
                if (string.IsNullOrEmpty(ServiceLocation.ApiServerUrl)) throw new DatabankException("ServiceLocation.ApiServerUrl cannot be null or empty!");

                var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);

                cookieContainer = new CookieContainer();
                sharedHandler = new HttpClientHandler { CookieContainer = cookieContainer, UseCookies = true };
                #pragma warning disable S1075 // This is fine, since it's standard in URLs
                var baseUrl = ServiceLocation.ApiServerUrl.TrimEnd('/') + "/";
                #pragma warning restore S1075

                httpClient?.Dispose();
                httpClient = new HttpClient(sharedHandler, disposeHandler: false) { BaseAddress = new Uri(baseUrl, UriKind.Absolute) };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The first authenticated request establishes the OnBase session (and
                // consumes a license) and returns the Cookie.Session.OnBase.Hyland
                // cookie, captured automatically by the handler's CookieContainer above.
                using var response = await httpClient.GetAsync("document-type-groups").ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new DatabankException($"Failed to establish OnBase session: request to [document-type-groups] returned {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                IsConnected = true;

                // The Forms API client is built here too, sharing sharedHandler/cookieContainer,
                // but only if a Forms API URL is actually configured, not every deployment
                // needs Unity Form lookups.
                formsHttpClient?.Dispose();
                formsHttpClient = null;
                if (!string.IsNullOrEmpty(ServiceLocation.FormsApiUrl))
                {
                    #pragma warning disable S1075
                    var formsBaseUrl = ServiceLocation.FormsApiUrl.TrimEnd('/') + "/";
                    #pragma warning restore S1075
                    formsHttpClient = new HttpClient(sharedHandler, disposeHandler: false) { BaseAddress = new Uri(formsBaseUrl, UriKind.Absolute) };
                    formsHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    formsHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                if (ServiceLocation.KeepAlive) StartHeartbeat();
            }
            catch (DatabankException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to OnBase!", ex);
            }
        }

        /// <summary>
        /// Disconnects from OnBase, releasing the license held by the current session.
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                StopHeartbeat();

                if (httpClient != null && IsConnected)
                {
                    using var response = await httpClient.PostAsync("session/disconnect", null).ConfigureAwait(false);
                    // Not treated as fatal: the session's own cookie may already have
                    // expired (idle >5 minutes with KeepAlive off), in which case the
                    // session is already effectively gone regardless of this response.
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error disconnecting from OnBase!", ex);
            }
            finally
            {
                IsConnected = false;
                httpClient?.Dispose();
                httpClient = null;
                formsHttpClient?.Dispose();
                formsHttpClient = null;
                sharedHandler?.Dispose();
                sharedHandler = null;
                cookieContainer = null;
            }
        }

        /// <summary>
        /// Returns the connected, fully-configured (Bearer token + session cookie)
        /// HttpClient for making Document Management API requests. Used by
        /// RestApi.02-04's own helper classes.
        /// </summary>
        /// <returns>The connected HttpClient.</returns>
        public HttpClient GetHttpClient()
        {
            return IsConnected && httpClient != null
                ? httpClient
                : throw new DatabankException("Not connected! Call ConnectAsync() first.");
        }

        /// <summary>
        /// Returns the connected, fully-configured (Bearer token + shared session cookie
        /// container) HttpClient for making Forms API requests (Unity Form/E-Form
        /// lookups). Only available when <see cref="Configuration.ServiceLocation.FormsApiUrl"/>
        /// is configured; see this class's own Training Notes for why it shares the same
        /// underlying HttpClientHandler as <see cref="GetHttpClient"/>'s own client.
        /// </summary>
        /// <returns>The connected HttpClient for the Forms API.</returns>
        public HttpClient GetFormsHttpClient()
        {
            return IsConnected && formsHttpClient != null
                ? formsHttpClient
                : throw new DatabankException("Not connected to the Forms API! Call ConnectAsync() first, with ServiceLocation.FormsApiUrl configured.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed) return;

            StopHeartbeat();
            httpClient?.Dispose();
            formsHttpClient?.Dispose();
            sharedHandler?.Dispose();

            disposed = true;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        // Obtains a Bearer token, using whichever AuthenticationMode is configured
        private Task<string> GetAccessTokenAsync()
        {
            return ServiceLocation.AuthenticationMode switch
            {
                AuthenticationMode.OnBaseCredentials => IdpAuthentication.GetAccessTokenAsync(IdpSettings, ServiceLocation.DecryptedUsername, ServiceLocation.DecryptedPassword),
                AuthenticationMode.DomainCredentials => throw new NotImplementedException("AuthenticationMode.DomainCredentials is not yet implemented in this training set; no documented Hyland IdP grant type equivalent was found. See AuthenticationMode's own Training Notes and LectureNotes.md."),
                AuthenticationMode.AccessToken => throw new NotImplementedException("AuthenticationMode.AccessToken is not yet implemented in this training set, even though it would be trivial (use the supplied token directly). Stubbed for consistency with the other non-OnBaseCredentials modes. See AuthenticationMode's own Training Notes and LectureNotes.md."),
                AuthenticationMode.SingleSignOn => throw new NotImplementedException("AuthenticationMode.SingleSignOn is not yet implemented in this training set; it maps conceptually to Authorization Code with PKCE, which requires a browser-based redirect flow not yet built. See AuthenticationMode's own Training Notes and LectureNotes.md."),
                _ => throw new DatabankException($"Unsupported AuthenticationMode '{ServiceLocation.AuthenticationMode}'!"),
            };
        }

        // Starts the periodic background heartbeat that keeps the session's cookie from
        // expiring during idle periods
        private void StartHeartbeat()
        {
            StopHeartbeat();
            heartbeatTimer = new Timer(SendHeartbeat, null, HeartbeatInterval, HeartbeatInterval);
        }

        // Stops the periodic background heartbeat, if running
        private void StopHeartbeat()
        {
            heartbeatTimer?.Dispose();
            heartbeatTimer = null;
        }

        // Timer callback: sends a single heartbeat. Failures are swallowed rather than
        // thrown, there's no caller on the other end of a background timer callback to
        // catch/handle an exception, and one missed heartbeat shouldn't crash the process
        // if the session has genuinely expired, the next real API call will fail loudly
        // and visibly instead.
        private async void SendHeartbeat(object state)
        {
            try
            {
                if (httpClient != null && IsConnected)
                {
                    using var response = await httpClient.PostAsync("session/heartbeat", null).ConfigureAwait(false);
                }
            }
            catch
            {
                // Swallowed deliberately, see method summary above.
            }
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
