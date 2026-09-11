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

namespace RestApi._00.CommonFunctionality.Models.Enumerations
{
    #region Training Notes
    /*
     * *Migration Note: same four member names as Unity.00.CommonFunctionality's own
     * AuthenticationMode, deliberately, for UI/feature parity (RestApi.TestHarness's
     * Settings page has the exact same Authentication Mode dropdown Unity.TestHarness
     * does). But the REST API has a fundamentally different auth model than the Unity
     * API: everything goes through the Hyland IdP via OAuth2, there's no direct
     * equivalent of Unity API's four separate AuthenticationProperties-derived types.
     * Each member here instead maps to a different OAuth2 grant type / IdP interaction
     * pattern:
     *
     *   OnBaseCredentials - OAuth2 "password" grant (Resource Owner Password
     *                       Credentials): username/password sent directly to the IdP's
     *                       token endpoint. The only mode actually implemented right
     *                       now, see RestApi.01.ConnectingToOnBase's IdpAuthentication.
     *                       Closest functional match to what this mode does in the Unity
     *                       API version (credentials entered directly in the app, no
     *                       browser redirect).
     *
     *   DomainCredentials - STUBBED. The Hyland IdP documentation reviewed for this
     *                       training set doesn't describe a direct Windows-integrated/
     *                       Kerberos grant type; NT authentication in the Unity API
     *                       sense (the OS identity the process is already running as)
     *                       doesn't have a documented REST API equivalent. Left as an
     *                       explicit NotImplementedException rather than guessed at. See
     *                       LectureNotes.md.
     *
     *   AccessToken       - STUBBED, even though a user directly supplying an
     *                       already-obtained token needs no grant flow at all (it would
     *                       be trivial to implement: use the supplied token as the
     *                       Bearer token directly). Stubbed anyway, for consistency:
     *                       matching every other non-OnBaseCredentials mode as an
     *                       explicit placeholder rather than implementing this one
     *                       opportunistically because it happened to be easy.
     *
     *   SingleSignOn      - STUBBED. Maps conceptually to "Authorization Code with
     *                       PKCE", the grant type the Authentication guide explicitly
     *                       recommends for SSO-capable user-facing applications. Not
     *                       implemented here: it requires a browser-based redirect flow,
     *                       a substantially bigger addition to a desktop test harness
     *                       than a straight credential swap. See LectureNotes.md.
     *
     * See RestApi.01.ConnectingToOnBase's SessionManagement and IdpAuthentication for
     * where these stubs actually live and throw.
     */
    #endregion

    /// <summary>
    /// The authentication mode <see cref="Configuration.ServiceLocation"/> should use to
    /// establish a NEW OnBase session via the Document Management (REST) API. Each member
    /// corresponds to a different OAuth2 grant type / Hyland Identity Provider (IdP)
    /// interaction pattern; only <see cref="OnBaseCredentials"/> is currently implemented,
    /// see this enum's own Training Notes for what the others map to and why they're
    /// stubbed.
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// OAuth2 "password" grant: OnBase username/password sent directly to the Hyland
        /// IdP's token endpoint. The only mode currently implemented.
        /// </summary>
        OnBaseCredentials = 0,

        /// <summary>
        /// Windows/NT domain authentication. Stubbed: no documented Hyland IdP grant type
        /// equivalent was found for this training set.
        /// </summary>
        DomainCredentials = 1,

        /// <summary>
        /// A pre-obtained Hyland IdP access token, used directly. Stubbed for consistency
        /// with the other non-<see cref="OnBaseCredentials"/> modes, even though this one
        /// would require no grant flow to implement.
        /// </summary>
        AccessToken = 2,

        /// <summary>
        /// Single Sign-On, mapping conceptually to the "Authorization Code with PKCE"
        /// grant type. Stubbed: requires a browser-based redirect flow.
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
