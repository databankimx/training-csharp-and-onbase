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

namespace Unity._00.CommonFunctionality.Models.Enumerations
{
    #region Training Notes
    /*
     * *Migration Note: new in this training set, replacing ServiceLocation's original
     * UseNTAuthentication bool. Hyland.Unity.AuthenticationProperties is actually an
     * abstract base class with FIVE concrete derived types (Hyland.Unity docs,
     * "AuthenticationProperties Class"), but this enum has only FOUR members, not five.
     *
     *   OnBaseAuthenticationProperties       - traditional username/password
     *   DomainAuthenticationProperties       - Windows/NT authentication
     *   AccessTokenAuthenticationProperties  - Hyland Identity Provider (IdP) token
     *   SingleSignOnAuthenticationProperties - SSO, using a separately-issued license token
     *   SessionIDAuthenticationProperties    - reconnect to an already-established session
     *                                           (NOT represented here, see below)
     *
     * SessionIDAuthenticationProperties is deliberately NOT a member of this enum. A
     * session-ID reconnect isn't really an independent way to establish credentials,
     * it's an attempt to resume a PREVIOUS connection made via one of the four modes
     * above, and if that resume fails, the only sensible fallback is to actually
     * establish a NEW session using real credentials again, which requires
     * AuthenticationMode to still be holding one of those four values, not "SessionId".
     * An earlier version of this enum included SessionId as a fifth member; that made
     * failover impossible to express correctly (if the mode itself IS "SessionId" and
     * reconnecting fails, there is nothing left to fall back to, without a bug: retrying
     * the same failed reconnect in an infinite loop). See Configuration.ServiceLocation's
     * own SessionId property (now independent of AuthenticationMode) and
     * Unity.01.ConnectingToOnBase's Connect() for how the two combine correctly.
     */
    #endregion

    /// <summary>
    /// The Unity API authentication mode <see cref="Configuration.ServiceLocation"/>
    /// should use to establish a NEW OnBase session. Each member corresponds directly to
    /// one of four of Hyland.Unity's five <c>AuthenticationProperties</c>-derived types;
    /// the fifth, session-ID reconnection, is handled separately (see
    /// <see cref="Configuration.ServiceLocation.SessionId"/>), since it isn't a way of
    /// establishing NEW credentials, only of resuming a previous connection made with one
    /// of the modes below.
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// Traditional OnBase username/password authentication
        /// (<c>Hyland.Unity.OnBaseAuthenticationProperties</c>). The default, and the
        /// only mode this configuration schema originally supported.
        /// </summary>
        OnBaseCredentials = 0,

        /// <summary>
        /// Windows/NT domain authentication (<c>Hyland.Unity.DomainAuthenticationProperties</c>).
        /// </summary>
        DomainCredentials = 1,

        /// <summary>
        /// Hyland Identity Provider (IdP) access token authentication
        /// (<c>Hyland.Unity.AccessTokenAuthenticationProperties</c>).
        /// </summary>
        AccessToken = 2,

        /// <summary>
        /// Single Sign-On authentication, using a separately-issued license token
        /// (<c>Hyland.Unity.SingleSignOnAuthenticationProperties</c>).
        /// </summary>
        SingleSignOn = 3
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
