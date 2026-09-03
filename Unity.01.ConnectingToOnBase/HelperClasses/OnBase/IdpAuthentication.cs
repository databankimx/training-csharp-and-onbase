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
using System.Net.Http;
using System.Text.Json;
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._01.ConnectingToOnBase.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: this is based on the Hyland Unity API documentation's own
     * "Connecting with Hyland IdP" sample, adapted here with a few deliberate
     * corrections/substitutions over the raw sample, each noted inline below:
     *
     * 1. FormUrlEncodedContent, not a hand-built query string. The documented sample
     *    builds the request body as a raw interpolated string
     *    ($"grant_type={_grantType}&username={_username}&...") and only afterward forces
     *    the Content-Type header to "application/x-www-form-urlencoded", it never
     *    actually percent-encodes the individual values. A username, password, or client
     *    secret containing a "&", "=", or "%" character would silently corrupt the
     *    request. FormUrlEncodedContent (below) encodes each value correctly.
     *
     * 2. System.Text.Json, not Newtonsoft.Json. The documented sample uses
     *    Newtonsoft.Json.Linq.JObject to parse the response, an extra NuGet dependency
     *    this training set has no other reason to take on. System.Text.Json (built into
     *    the .NET/`net48`-compatible BCL surface via a small NuGet package) parses the
     *    same JSON equally well for this simple "read one field" use case.
     *
     * 3. The documented sample's catch block references a misspelled type, "Execption",
     *    which wouldn't compile as written, corrected here to DatabankException, matching
     *    this training set's own exception-handling convention throughout.
     *
     * Only the "password" grant type is implemented, matching the one example the
     * documentation actually provides. SAML/ADFS/other grant types are left as explicit
     * stubs (NotImplementedException) rather than guessed at, since their exact request
     * shapes aren't derivable from the documentation reviewed for this training set. See
     * LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// Obtains an OnBase access token from the Hyland Identity Provider (IdP), for use
    /// with <see cref="Hyland.Unity.Application.CreateAccessTokenAuthenticationProperties"/>.
    /// </summary>
    public static class IdpAuthentication
    {
        #region Public Methods
        /// <summary>
        /// Obtains an access token from the Hyland IdP, using whichever grant type is
        /// configured on <paramref name="idpSettings"/>.
        /// </summary>
        /// <param name="idpSettings">The Hyland IdP settings to use.</param>
        /// <param name="username">The OnBase username, required by the "password" grant type.</param>
        /// <param name="password">The decrypted OnBase password, required by the "password" grant type.</param>
        /// <returns>The obtained access token.</returns>
        public static string GetAccessToken(IdpSettings idpSettings, string username, string password)
        {
            if (idpSettings == null) throw new DatabankException("IdpSettings cannot be null!");

            return (idpSettings.IdpGrantType?.Trim().ToLowerInvariant()) switch
            {
                "password" => GetAccessTokenViaPasswordGrant(idpSettings, username, password),
                "saml" or "saml2" => throw new NotImplementedException("The 'saml' IdpGrantType is not yet implemented in this training set; its exact request shape wasn't derivable from the documentation reviewed. See LectureNotes.md."),
                "adfs" => throw new NotImplementedException("The 'adfs' IdpGrantType is not yet implemented in this training set; its exact request shape wasn't derivable from the documentation reviewed. See LectureNotes.md."),
                "client_credentials" => throw new NotImplementedException("The 'client_credentials' IdpGrantType is not yet implemented in this training set; its exact request shape wasn't derivable from the documentation reviewed. See LectureNotes.md."),
                _ => throw new DatabankException($"Unsupported or unrecognized IdpGrantType '{idpSettings.IdpGrantType}'!"),
            };
        }
        #endregion

        #region Private Methods
        // Obtains an access token via the OAuth2 Resource Owner Password Credentials
        // grant, matching the Unity API documentation's own "Connecting with Hyland IdP"
        // example, with the corrections noted in Training Notes above.
        private static string GetAccessTokenViaPasswordGrant(IdpSettings idpSettings, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(idpSettings.IdpUrl) || string.IsNullOrEmpty(idpSettings.IdpTenant) ||
                    string.IsNullOrEmpty(idpSettings.IdpClientId) || string.IsNullOrEmpty(idpSettings.DecryptedIdpClientSecret))
                {
                    throw new DatabankException("IdpUrl, IdpTenant, IdpClientId, and IdpClientSecret must all be configured to obtain an access token via the 'password' grant type.");
                }

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new DatabankException("A username and password are required to obtain an access token via the 'password' grant type.");
                }

                using var idpClient = new HttpClient { BaseAddress = new Uri(idpSettings.IdpUrl, UriKind.Absolute) };

                var formValues = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", idpSettings.IdpGrantType),
                    new("username", username),
                    new("password", password),
                    new("scope", idpSettings.IdpScope),
                    new("client_id", idpSettings.IdpClientId),
                    new("client_secret", idpSettings.DecryptedIdpClientSecret),
                    new("tenant", idpSettings.IdpTenant)
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, idpSettings.IdpUrl)
                {
                    Content = new FormUrlEncodedContent(formValues)
                };
                request.Headers.Add("Accept", "application/json");

                using var response = idpClient.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new DatabankException($"IdP token request to [{idpSettings.IdpUrl}] failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                var responseBody = response.Content.ReadAsStringAsync().Result;

                using var document = JsonDocument.Parse(responseBody);
                if (!document.RootElement.TryGetProperty("access_token", out var accessTokenElement))
                {
                    throw new DatabankException($"IdP response did not contain an access_token: {responseBody}");
                }

                return accessTokenElement.GetString();
            }
            catch (DatabankException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error obtaining Hyland IdP access token!", ex);
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
