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
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._00.CommonFunctionality.Models.Objects;
using SysConfig = System.Configuration;
#endregion

#pragma warning disable S125 // Commented code permitted in lesson files
namespace Unity._01.ConnectingToOnBase.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *DRAFT, NOT FINAL* - see LectureNotes.md's "Draft, Not Final" section. This is a
     * first pass at extending ConnectNewSession() to switch on the AuthenticationMode
     * enum instead of the original UseNTAuthentication bool. Both items below are now
     * confirmed against the actual Unity API / worked through:
     *
     * 1. DomainCredentials: CONFIRMED - CreateDomainAuthenticationProperties only accepts
     *    (url, datasource), no credentials parameter of any kind. NT authentication is
     *    purely the Windows identity the process is already running as; there is no
     *    Unity API mechanism for an alternate domain user (that would require OS-level
     *    impersonation performed before this call, a separate concern entirely).
     *
     * 2. SessionId: RESOLVED - it is NOT a member of AuthenticationMode at all (see that
     *    enum's own Training Notes for why). Instead, Connect() below checks
     *    ServiceLocation.SessionId directly, FIRST, before ever looking at
     *    AuthenticationMode: if a session ID is configured, a reconnect is always
     *    attempted first (this only succeeds if that session's own IsDisconnectEnabled
     *    is false, i.e., it was originally connected with KeepAlive set), and
     *    ReconnectExistingSession()'s existing failover logic falls back to
     *    ConnectNewSession() (using whichever AuthenticationMode IS configured) if the
     *    reconnect fails and AllowSessionFailover is true. An earlier draft made SessionId
     *    a fifth AuthenticationMode value and had ConnectNewSession() call
     *    ReconnectExistingSession() directly for it, which would have caused infinite
     *    recursion the moment that reconnect failed with failover enabled (fail →
     *    fall back to ConnectNewSession() → hit the SAME SessionId case again → fail
     *    again → ...). This design has no such cycle: ConnectNewSession() never touches
     *    SessionId at all.
     */
    #endregion

    /// <summary>
    /// Manage, connect, and disconnect OnBase sessions
    /// </summary>
    public static class SessionManagement
    {
        #region Properties
        /// <summary>
        /// OnBase Connection Settings
        /// </summary>
        public static ServiceLocation ServiceLocation { get; set; }

        /// <summary>
        /// Hyland Identity Provider (IdP) Settings, used by <see cref="GetAccessTokenIfNeeded"/>
        /// when <see cref="ServiceLocation"/>'s AuthenticationMode is AccessToken and no
        /// token was supplied directly.
        /// </summary>
        public static IdpSettings IdpSettings { get; set; }
        #endregion

        #region Static Constructors
        // On first access, load the connection settings from the XML config file
        static SessionManagement()
        {
            var settings = (OnBaseSettings)SysConfig.ConfigurationManager.GetSection(OnBaseSettings.SectionName);
            ServiceLocation = settings.ServiceLocation;
            IdpSettings = settings.IdpSettings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to OnBase
        /// </summary>
        /// <param name="serviceLocation">OnBase Connection Settings</param>
        /// <param name="sessionId">Existing Session ID</param>
        /// <returns>Unity API Application Object</returns>
        public static Application Connect(this ServiceLocation serviceLocation, string sessionId = null)
        {
            try
            {
                ServiceLocation = serviceLocation;

                // If we are given an existing session ID, try to reconnect
                return string.IsNullOrEmpty(sessionId) ? Connect() : Connect(sessionId);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to OnBase!", ex);
            }
        }

        /// <summary>
        /// Connect to OnBase
        /// </summary>
        /// <param name="sessionId">Existing Session ID</param>
        /// <returns>Unity API Application Object</returns>
        public static Application Connect(this string sessionId)
        {
            try
            {
                return ReconnectExistingSession(sessionId, ServiceLocation.KeepAlive);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to OnBase!", ex);
            }
        }

        /// <summary>
        /// Connect to OnBase. If <see cref="Configuration.ServiceLocation.SessionId"/> is
        /// configured, a reconnect to that session is attempted FIRST, regardless of
        /// <see cref="Configuration.ServiceLocation.AuthenticationMode"/>; a brand new
        /// session is only established (per AuthenticationMode) if there's no configured
        /// SessionId, or if reconnecting to it fails and AllowSessionFailover is true.
        /// </summary>
        /// <returns>Unity API Application Object</returns>
        public static Application Connect()
        {
            try
            {
                return !string.IsNullOrEmpty(ServiceLocation.SessionId)
                    ? ReconnectExistingSession(ServiceLocation.SessionId, ServiceLocation.KeepAlive)
                    : ConnectNewSession();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error connecting to OnBase!", ex);
            }
        }

        /// <summary>
        /// Disconnect from OnBase
        /// </summary>
        /// <param name="app">Unity API Application Object</param>
        public static void Disconnect(this Application app)
        {
            try
            {
                // Before we can disconnect a maintained session, we have to reconnect in disconnectable mode
                if (!app.IsDisconnectEnabled) app = ReconnectExistingSession(app.SessionID, false);
                app.Disconnect();
            }
            catch (Exception ex)
            {
                if (app == null) throw new DatabankException("Cannot disconnect NULL application!", ex);
                throw new DatabankException($"Error disconnecting application with session [{app.SessionID}]!", ex);
            }
        }

        /// <summary>
        /// Disconnect from OnBase
        /// </summary>
        /// <param name="sessionId">OnBase Session ID</param>
        public static void Disconnect(this string sessionId)
        {
            try
            {
                // Reconnect to the existing session to allow disconnecting
                var app = ReconnectExistingSession(sessionId, false);
                app.Disconnect();
            }
            catch (Exception ex)
            {
                throw new DatabankException($"Error disconnecting application with session [{sessionId}]!", ex);
            }
        }
        #endregion

        #region Private Methods
        // Re-establish an existing OnBase session
        private static Application ReconnectExistingSession(string sessionId, bool keepAlive = true)
        {
            try
            {
                if (ServiceLocation == null)
                    throw new DatabankException("ServiceLocation cannot be null!");

                // Reconnect to the existing session setting IsDisconnectEnabled appropriately
                var authProps = Application.CreateSessionIDAuthenticationProperties(ServiceLocation.ServicePath, sessionId, !keepAlive);

                // If using a Unity Integration GUID, set the identity
                if (!string.IsNullOrEmpty(ServiceLocation.ApplicationId))
                    authProps.IdentitySettings = authProps.CreateIdentitySettings(ServiceLocation.ApplicationId);

                return Application.Connect(authProps);
            }
            catch (Exception ex)
            {
                // Attempt to fail over to a new session when reconnecting fails (if permitted)
                if (ServiceLocation != null && ServiceLocation.AllowSessionFailover) return ConnectNewSession();

                // Otherwise, error - connection failed
                throw new DatabankException($"Failed to reconnect session [{sessionId}]!", ex);
            }
        }

        // Establish a new OnBase session, using whichever AuthenticationMode is configured
        private static Application ConnectNewSession()
        {
            try
            {
                if (ServiceLocation == null) throw new DatabankException("ServiceLocation cannot be null!");

                AuthenticationProperties authProps;

                switch (ServiceLocation.AuthenticationMode)
                {
                    case AuthenticationMode.OnBaseCredentials:
                        authProps = Application.CreateOnBaseAuthenticationProperties(ServiceLocation.ServicePath, ServiceLocation.DecryptedUsername, ServiceLocation.DecryptedPassword, ServiceLocation.DataSource);
                        break;

                    case AuthenticationMode.DomainCredentials:
                        authProps = Application.CreateDomainAuthenticationProperties(ServiceLocation.ServicePath, ServiceLocation.DataSource);
                        break;

                    case AuthenticationMode.AccessToken:
                        authProps = Application.CreateAccessTokenAuthenticationProperties(ServiceLocation.ServicePath, GetAccessTokenIfNeeded(), ServiceLocation.DataSource);
                        break;

                    case AuthenticationMode.SingleSignOn:
                        authProps = Application.CreateSingleSignOnAuthenticationProperties(ServiceLocation.ServicePath, ServiceLocation.DataSource);
                        authProps.LicenseToken = ServiceLocation.DecryptedLicenseToken;
                        break;

                    default:
                        throw new DatabankException($"Unsupported AuthenticationMode '{ServiceLocation.AuthenticationMode}'!");
                }

                // We need to set the license type if using query metering or enterprise connection
                authProps.LicenseType = ServiceLocation.LicenseType;

                // If you want to be able to reconnect to the session, IsDisconnectEnabled must be false
                authProps.IsDisconnectEnabled = !ServiceLocation.KeepAlive;

                // If using a Unity Integration GUID, set the identity
                if (!string.IsNullOrEmpty(ServiceLocation.ApplicationId))
                    authProps.IdentitySettings = authProps.CreateIdentitySettings(ServiceLocation.ApplicationId);

                return Application.Connect(authProps);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Failed to connect new OnBase session!", ex);
            }
        }


        // Returns `ServiceLocation.DecryptedAccessToken` directly if one was supplied;
        // otherwise obtains one from the Hyland IdP using IdpSettings and
        // ServiceLocation's own Username/DecryptedPassword.
        private static string GetAccessTokenIfNeeded()
        {
            if (!string.IsNullOrEmpty(ServiceLocation.DecryptedAccessToken))
                return ServiceLocation.DecryptedAccessToken;

            return IdpAuthentication.GetAccessToken(IdpSettings, ServiceLocation.DecryptedUsername, ServiceLocation.DecryptedPassword);
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
