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
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Configuration;
using RestApi._00.CommonFunctionality.Models.Objects;
#endregion

namespace RestApi._01.ConnectingToOnBase.HelperClasses.OnBase
{
    #region Training Notes
    /*
     * *Migration Note: adapted from Unity.01.ConnectingToOnBase's own IdpAuthentication,
     * which is itself based on the Hyland Unity API documentation's "Connecting with
     * Hyland IdP" sample (FormUrlEncodedContent instead of a hand-built, unencoded query
     * string; System.Text.Json instead of Newtonsoft.Json; DatabankException instead of
     * the documented sample's misspelled/non-compiling "Execption" type). Those same
     * corrections carry over here unchanged, since the Hyland IdP token endpoint itself
     * (grant_type=password) is identical infrastructure regardless of which OnBase API
     * (Unity or REST) the resulting token is ultimately used against.
     *
     * Genuinely different from Unity.01's version: async/await throughout, not
     * .Result-blocking calls. Unity API's own SDK is synchronous by design (a SOAP/WCF
     * client wrapped in a purely synchronous surface), so Unity.01 matches that
     * synchronous shape throughout its own call chain. This project has no such
     * constraint, an HTTP token request is exactly the kind of I/O-bound work async/await
     * exists for, and RestApi.TestHarness's own UI code can await it without blocking.
     *
     * Only the "password" grant type is implemented, matching the one example the
     * documentation actually provides. SAML/ADFS/client_credentials grant types are left
     * as explicit stubs (NotImplementedException) rather than guessed at, since their
     * exact request shapes aren't derivable from the documentation reviewed for this
     * training set. See LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// Obtains an OnBase access token from the Hyland Identity Provider (IdP), for use as
    /// the Bearer token on requests to the Document Management (REST) API.
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
        public static Task<string> GetAccessTokenAsync(IdpSettings idpSettings, string username, string password)
        {
            if (idpSettings == null) throw new DatabankException("IdpSettings cannot be null!");

            return (idpSettings.IdpGrantType?.Trim().ToLowerInvariant()) switch
            {
                "password" => GetAccessTokenViaPasswordGrantAsync(idpSettings, username, password),
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
        private static async Task<string> GetAccessTokenViaPasswordGrantAsync(IdpSettings idpSettings, string username, string password)
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

                using var response = await idpClient.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new DatabankException($"IdP token request to [{idpSettings.IdpUrl}] failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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
