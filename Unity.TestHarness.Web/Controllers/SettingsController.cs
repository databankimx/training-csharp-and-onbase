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
using System.Web.Configuration;
using System.Web.Mvc;
using Hyland.Unity;
using Unity._00.CommonFunctionality.Models.Configuration;
using Enums = Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._01.ConnectingToOnBase.HelperClasses.OnBase;
using Unity.TestHarness.Web.Infrastructure;
using Unity.TestHarness.Web.Models;
using SysConfig = System.Configuration;
#endregion

namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Settings
     * page (SettingsViewModel + its View). Same two-action split as the WPF version:
     * Apply pushes edited values into SessionManagement.ServiceLocation/IdpSettings
     * in-memory only; Save to Web.config additionally writes them back to disk via
     * WebConfigurationManager (the web equivalent of the WPF version's
     * ConfigurationManager.OpenExeConfiguration), so they persist across app pool
     * recycles/restarts.
     *
     * SessionManagement.ServiceLocation/IdpSettings are STATIC, process-wide properties,
     * not per-session state, unlike everything else this web app holds in Session. That
     * means Settings changes here affect every user of the app simultaneously, not just
     * the person editing them, this was confirmed as an acceptable, deliberate design for
     * this harness (an admin-style global configuration page), not an oversight.
     *
     * Secret fields (Password, AccessToken, LicenseToken, IdpClientSecret) use
     * type="password" inputs in the View, same concealment the WPF version's real
     * PasswordBox controls provide, though a browser's own password-manager/autofill
     * behavior is a different threat model than a desktop PasswordBox, worth being aware
     * of if this is ever exposed beyond a trusted internal network.
     */
    #endregion

    /// <summary>
    /// The Settings page: edits SessionManagement.ServiceLocation/IdpSettings and DocPop,
    /// either applying changes in-memory or saving them back to Web.config.
    /// </summary>
    public class SettingsController : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Settings page, populated from whatever's currently applied.
        /// </summary>
        /// <returns>The Settings page.</returns>
        public ActionResult Index()
        {
            return View(BuildModel());
        }

        /// <summary>
        /// Applies the edited values to SessionManagement.ServiceLocation/IdpSettings
        /// in-memory, for this run only.
        /// </summary>
        /// <param name="model">The edited settings.</param>
        /// <returns>A redirect back to the Settings page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(SettingsPageModel model)
        {
            ApplyInternal(model);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Applies the edited values (same as <see cref="Apply"/>) AND writes them back
        /// to Web.config on disk, so they persist across app pool recycles/restarts.
        /// </summary>
        /// <param name="model">The edited settings.</param>
        /// <returns>A redirect back to the Settings page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveToConfig(SettingsPageModel model)
        {
            if (!ApplyInternal(model)) return RedirectToAction("Index");

            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");
                var section = (OnBaseSettings)config.GetSection(OnBaseSettings.SectionName);

                section.ServiceLocation.ApplicationId = model.ApplicationId;
                section.ServiceLocation.ServicePath = model.ServicePath;
                section.ServiceLocation.DataSource = model.DataSource;
                section.ServiceLocation.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), model.LicenseType);
                section.ServiceLocation.AuthenticationMode = (Enums.AuthenticationMode)Enum.Parse(typeof(Enums.AuthenticationMode), model.AuthenticationMode);
                section.ServiceLocation.Username = model.Username;
                section.ServiceLocation.Password = model.Password;
                section.ServiceLocation.AccessToken = model.AccessToken;
                section.ServiceLocation.LicenseToken = model.LicenseToken;
                section.ServiceLocation.SessionId = model.SessionId;

                section.DocPop.DocPopBaseUrl = model.DocPopBaseUrl;
                section.DocPop.DocPopChecksumSeed = model.DocPopChecksumSeed;

                section.IdpSettings.IdpUrl = model.IdpUrl;
                section.IdpSettings.IdpTenant = model.IdpTenant;
                section.IdpSettings.IdpClientId = model.IdpClientId;
                section.IdpSettings.IdpClientSecret = model.IdpClientSecret;
                section.IdpSettings.IdpScope = model.IdpScope;
                section.IdpSettings.IdpGrantType = model.IdpGrantType;

                config.Save(SysConfig.ConfigurationSaveMode.Modified);
                SysConfig.ConfigurationManager.RefreshSection(OnBaseSettings.SectionName);

                SessionLog.Success("Settings saved to Web.config.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Read the current settings from SessionManagement/OnBaseSettings into a page model
        private static SettingsPageModel BuildModel()
        {
            var serviceLocation = SessionManagement.ServiceLocation;
            var idpSettings = SessionManagement.IdpSettings;
            var onBaseSettings = (OnBaseSettings)SysConfig.ConfigurationManager.GetSection(OnBaseSettings.SectionName);

            return new SettingsPageModel
            {
                ApplicationId = serviceLocation?.ApplicationId,
                ServicePath = serviceLocation?.ServicePath,
                DataSource = serviceLocation?.DataSource,
                LicenseType = (serviceLocation?.LicenseType ?? Hyland.Unity.LicenseType.Default).ToString(),
                AuthenticationMode = (serviceLocation?.AuthenticationMode ?? Enums.AuthenticationMode.OnBaseCredentials).ToString(),
                Username = serviceLocation?.Username,
                Password = serviceLocation?.Password,
                AccessToken = serviceLocation?.AccessToken,
                LicenseToken = serviceLocation?.LicenseToken,
                SessionId = serviceLocation?.SessionId,
                KeepAlive = serviceLocation?.KeepAlive ?? false,
                AllowSessionFailover = serviceLocation?.AllowSessionFailover ?? false,

                DocPopBaseUrl = onBaseSettings?.DocPop?.DocPopBaseUrl,
                DocPopChecksumSeed = onBaseSettings?.DocPop?.DocPopChecksumSeed,

                IdpUrl = idpSettings?.IdpUrl,
                IdpTenant = idpSettings?.IdpTenant,
                IdpClientId = idpSettings?.IdpClientId,
                IdpClientSecret = idpSettings?.IdpClientSecret,
                IdpScope = idpSettings?.IdpScope ?? "evolution",
                IdpGrantType = idpSettings?.IdpGrantType ?? "password"
            };
        }

        // Push edited values into SessionManagement.ServiceLocation/IdpSettings (in-memory only)
        private static bool ApplyInternal(SettingsPageModel model)
        {
            try
            {
                var serviceLocation = new ServiceLocation
                {
                    ApplicationId = model.ApplicationId,
                    ServicePath = model.ServicePath,
                    DataSource = model.DataSource,
                    LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), model.LicenseType),
                    AuthenticationMode = (Enums.AuthenticationMode)Enum.Parse(typeof(Enums.AuthenticationMode), model.AuthenticationMode),
                    Username = model.Username,
                    Password = model.Password,
                    AccessToken = model.AccessToken,
                    LicenseToken = model.LicenseToken,
                    SessionId = model.SessionId,
                    KeepAlive = model.KeepAlive,
                    AllowSessionFailover = model.AllowSessionFailover
                };

                // Constructing a ServiceLocation directly never runs PostDeserialize(), so
                // this is called explicitly to get the same "AuthenticationMode 'X'
                // requires Y" validation Web.config loading gets for free.
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

                SessionManagement.ServiceLocation = serviceLocation;
                SessionManagement.IdpSettings = idpSettings;

                SessionLog.Success("Settings applied for this session.");
                return true;
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
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
