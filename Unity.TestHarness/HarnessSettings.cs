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
using System.Configuration;
#endregion

namespace Unity.TestHarness
{
    #region Training Notes
    /*
     * *Migration Note: a small, dedicated helper, kept separate from MainViewModel
     * specifically so the ConfigurationManager plumbing doesn't leak into the view
     * model. Unlike the OnBase connection settings (ServiceLocation/IdpSettings, which
     * the Settings page only writes back on an explicit "Save to App.config" click),
     * this ONE preference writes immediately on every toggle, it's harmless, low-stakes
     * UI state, not something that benefits from a deliberate "are you sure" save step.
     */
    #endregion

    /// <summary>
    /// Reads and writes the harness's own UI preferences (currently just
    /// <see cref="SetSidebarExpanded"/>) to/from App.config's &lt;appSettings&gt;.
    /// </summary>
    public static class HarnessSettings
    {
        #region Constants
        // The appSettings key
        private const string SidebarExpandedKey = "SidebarExpanded";
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads the sidebar's remembered expanded/collapsed state from App.config.
        /// Defaults to <see langword="true"/> (expanded) if the setting is missing or
        /// unreadable.
        /// </summary>
        /// <returns>The remembered state.</returns>
        public static bool GetSidebarExpanded()
        {
            var raw = ConfigurationManager.AppSettings[SidebarExpandedKey];
            return !bool.TryParse(raw, out var expanded) || expanded;
        }

        /// <summary>
        /// Writes the sidebar's expanded/collapsed state back to App.config, so it's
        /// remembered the next time the harness opens.
        /// </summary>
        /// <param name="expanded">The state to remember.</param>
        public static void SetSidebarExpanded(bool expanded)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings[SidebarExpandedKey] == null)
                {
                    config.AppSettings.Settings.Add(SidebarExpandedKey, expanded.ToString());
                }
                else
                {
                    config.AppSettings.Settings[SidebarExpandedKey].Value = expanded.ToString();
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                // Non-critical UI preference; if it can't be saved (read-only install
                // directory, etc.), the harness still works, it just won't remember the
                // preference next time.
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
