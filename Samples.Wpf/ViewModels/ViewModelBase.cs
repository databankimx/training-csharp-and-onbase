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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace Samples.Wpf.ViewModels
{
    /// <summary>
    /// Base class for any bindable ViewModel. Implements INotifyPropertyChanged, the
    /// mechanism WPF's data binding engine relies on to know a bound property's value
    /// changed and the UI needs to re-render. Contrast this against every web sample in
    /// this training set: there, a "changed" value simply gets rendered fresh on the next
    /// request/response, there's no long-lived UI state to notify.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Sets the backing field to a new value and raises a property change notification when the value changes.
        /// </summary>
        /// <remarks>Commonly used in property setters that implement change notification.</remarks>
        /// <typeparam name="T">The type of the field and value.</typeparam>
        /// <param name="field">The backing field to update.</param>
        /// <param name="value">The new value to assign.</param>
        /// <param name="propertyName">The name of the property to notify. The caller member name is used when omitted.</param>
        /// <returns><see langword="true"/> when the field value changed and a notification was raised; otherwise, <see
        /// langword="false"/>.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <remarks>Apply the CallerMemberName attribute to reduce hard-coded property name
        /// strings.</remarks>
        /// <param name="propertyName">The name of the changed property. Optional and automatically supplied by the caller when omitted.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
