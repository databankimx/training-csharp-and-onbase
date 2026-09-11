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

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own PlaceholderViewModel, purely
     * generic UI infrastructure with no Unity API/REST API dependency at all.
     */
    #endregion

    /// <summary>
    /// A stand-in for a page not yet built, so the sidebar navigation shell is fully
    /// functional (and testable) before every real page exists.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the PlaceholderViewModel class
    /// </remarks>
    /// <param name="pageName">The name of the page this stands in for.</param>
    public class PlaceholderViewModel(string pageName) : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The message to display.
        /// </summary>
        public string Message { get; } = $"{pageName} isn't built yet.";
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
