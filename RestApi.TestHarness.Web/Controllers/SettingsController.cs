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
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestApi._00.CommonFunctionality.Models.Configuration;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
#endregion

namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * SettingsController, but two genuine, worth-understanding behavioral differences
     * from it, both stemming from the same root cause (RestApi.01's SessionManagement is
     * no longer static, see its own LectureNotes.md):
     *
     * 1. Apply() here only affects the CURRENT user's own session (it edits
     *    store.GetOrCreate(HttpContext.Session.Id)'s own ServiceLocation/IdpSettings),
     *    not every user of the app simultaneously. Unity.TestHarness.Web's own Apply
     *    edited a truly static SessionManagement.ServiceLocation, so a change there was
     *    immediately visible to every connected user, an intentional, confirmed
     *    "admin-style global setting" design for that app. That specific behavior isn't
     *    reproducible anymore with a genuinely per-user SessionManagement, each user's
     *    own instance is its own separate object now.
     *
     * 2. SaveToConfig() writes to appsettings.json via direct file I/O (read the file as
     *    a JsonNode, update its own "RestApi" section, write it back), not through
     *    ASP.NET Core's IConfiguration API at all: IConfiguration is READ-ONLY by design
     *    (built for layered, multi-source configuration reading), unlike
     *    System.Configuration's own ConfigurationManager, which has a real Save() method
     *    for XML config sections. This means SaveToConfig persists to disk (so it
     *    survives an app restart), and, since SessionManagementStore itself was updated
     *    to read via IOptionsMonitor&lt;RestApiWebSettings&gt; (see its own Training
     *    Notes) rather than a fixed IOptions&lt;T&gt; snapshot, any NEW session created
     *    after the save picks up the new values as soon as ASP.NET Core's own
     *    appsettings.json file-watcher notices the change, no restart needed for that
     *    part. Sessions that already exist, though, keep whatever ServiceLocation/
     *    IdpSettings they were built with; SaveToConfig doesn't retroactively touch them.
     */
    #endregion

    /// <summary>
    /// The Settings page: edits the current session's own connection settings, either
    /// applying changes in-memory for that session, or additionally saving them to
    /// appsettings.json (affecting new sessions going forward, see this class's own
    /// Training Notes).
    /// </summary>
    /// <remarks>
    /// Create a new instance of the SettingsController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    /// <param name="sharedSettings">The shared, appsettings.json-bound connection infrastructure settings.</param>
    /// <param name="environment">Used to locate appsettings.json on disk for SaveToConfig.</param>
    public class SettingsController(SessionManagementStore store, SessionLogService log, IOptionsMonitor<RestApiWebSettings> sharedSettings, IWebHostEnvironment environment) : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Settings page, populated from the current session's own settings
        /// (or the shared defaults, if no session-specific ones exist yet).
        /// </summary>
        /// <returns>The Settings page.</returns>
        public IActionResult Index()
        {
            var session = store.TryGet(HttpContext.Session.Id);
            return View(BuildModel(session, sharedSettings.CurrentValue));
        }

        /// <summary>
        /// Applies the edited values to the current session's own connection settings,
        /// in-memory, for this session only (see this class's own Training Notes for how
        /// this differs from Unity.TestHarness.Web's own truly-global Apply).
        /// </summary>
        /// <param name="model">The edited settings.</param>
        /// <returns>A redirect back to the Settings page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(SettingsPageModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            ApplyInternal(model);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Applies the edited values (same as <see cref="Apply"/>) AND writes them back
        /// to appsettings.json on disk, so new sessions created afterward pick them up
        /// (see this class's own Training Notes for exactly what this does and doesn't
        /// affect).
        /// </summary>
        /// <param name="model">The edited settings.</param>
        /// <returns>A redirect back to the Settings page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveToConfig(SettingsPageModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            if (!ApplyInternal(model)) return RedirectToAction("Index");

            try
            {
                var path = Path.Combine(environment.ContentRootPath, "appsettings.json");
                var root = JsonNode.Parse(System.IO.File.ReadAllText(path)).AsObject();

                root["RestApi"] = new JsonObject
                {
                    ["ApiServerUrl"] = model.ApiServerUrl,
                    ["FormsApiUrl"] = model.FormsApiUrl,
                    ["KeepAlive"] = model.KeepAlive,
                    ["IdpUrl"] = model.IdpUrl,
                    ["IdpTenant"] = model.IdpTenant,
                    ["IdpClientId"] = model.IdpClientId,
                    ["IdpClientSecret"] = model.IdpClientSecret,
                    ["IdpScope"] = model.IdpScope,
                    ["IdpGrantType"] = model.IdpGrantType
                };

                System.IO.File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                log.Success("Settings saved to appsettings.json.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Build the page model from the given session's connection state, falling back to
        // the shared, appsettings.json-bound defaults if no session-specific
        // SessionManagement exists yet
        private static SettingsPageModel BuildModel(RestApi._01.ConnectingToOnBase.HelperClasses.OnBase.SessionManagement session, RestApiWebSettings sharedSettings)
        {
            var serviceLocation = session?.ServiceLocation;
            var idpSettings = session?.IdpSettings;

            return new SettingsPageModel
            {
                ApiServerUrl = serviceLocation?.ApiServerUrl ?? sharedSettings.ApiServerUrl,
                FormsApiUrl = serviceLocation?.FormsApiUrl ?? sharedSettings.FormsApiUrl,
                AuthenticationMode = (serviceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials).ToString(),
                Username = serviceLocation?.Username,
                Password = serviceLocation?.Password,
                AccessToken = serviceLocation?.AccessToken,
                LicenseToken = serviceLocation?.LicenseToken,
                KeepAlive = serviceLocation?.KeepAlive ?? sharedSettings.KeepAlive,

                IdpUrl = idpSettings?.IdpUrl ?? sharedSettings.IdpUrl,
                IdpTenant = idpSettings?.IdpTenant ?? sharedSettings.IdpTenant,
                IdpClientId = idpSettings?.IdpClientId ?? sharedSettings.IdpClientId,
                IdpClientSecret = idpSettings?.IdpClientSecret ?? sharedSettings.IdpClientSecret,
                IdpScope = idpSettings?.IdpScope ?? sharedSettings.IdpScope,
                IdpGrantType = idpSettings?.IdpGrantType ?? sharedSettings.IdpGrantType
            };
        }

        // Push edited values into the current session's own ServiceLocation/IdpSettings (in-memory only)
        private bool ApplyInternal(SettingsPageModel model)
        {
            try
            {
                var session = store.GetOrCreate(HttpContext.Session.Id);

                var serviceLocation = new ServiceLocation
                {
                    ApiServerUrl = model.ApiServerUrl,
                    FormsApiUrl = model.FormsApiUrl,
                    AuthenticationMode = (AuthenticationMode)Enum.Parse(typeof(AuthenticationMode), model.AuthenticationMode),
                    Username = model.Username,
                    Password = model.Password,
                    AccessToken = model.AccessToken,
                    LicenseToken = model.LicenseToken,
                    KeepAlive = model.KeepAlive
                };

                // Constructing a ServiceLocation directly never runs PostDeserialize(),
                // so this is called explicitly to get the same "AuthenticationMode 'X'
                // requires Y" validation appsettings.json loading gets for free.
                serviceLocation.Validate();

                var idpSettings = new IdpSettings
                {
                    IdpUrl = model.IdpUrl,
                    IdpTenant = model.IdpTenant,
                    IdpClientId = model.IdpClientId,
                    IdpClientSecret = model.IdpClientSecret,
                    IdpScope = model.IdpScope,
                    IdpGrantType = model.IdpGrantType
                };

                session.ServiceLocation = serviceLocation;
                session.IdpSettings = idpSettings;

                log.Success("Settings applied for this session.");
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
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
