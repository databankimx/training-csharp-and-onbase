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

namespace Unity.TestHarness.ViewModels
{
    /// <summary>
    /// Base class for all view models, providing standard <see cref="INotifyPropertyChanged"/> support.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Raised when a bound property's value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Sets <paramref name="field"/> to <paramref name="value"/> and raises
        /// <see cref="PropertyChanged"/> if the value actually changed.
        /// </summary>
        /// <typeparam name="T">The property's type.</typeparam>
        /// <param name="field">The backing field to update.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name (automatically supplied by the compiler).</param>
        /// <returns><see langword="true"/> if the value changed; otherwise, <see langword="false"/>.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property.
        /// </summary>
        /// <param name="propertyName">The property name (automatically supplied by the compiler).</param>
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
