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
using System.Windows;
using System.Windows.Controls;
#endregion

namespace Unity.TestHarness.Behaviors
{
    #region Training Notes
    /*
     * *Migration Note: PasswordBox.Password is deliberately NOT a DependencyProperty,
     * a security decision built into WPF itself (so a password can't accidentally end up
     * bound into a data-binding chain, logged, or otherwise leaked via ordinary property
     * inspection). BoundPassword here is the standard, well-established workaround: an
     * attached DependencyProperty that DOES support {Binding}, kept in sync with the real
     * PasswordBox.Password via the PasswordChanged event. IsUpdating guards against the
     * two directions of sync re-triggering each other in an infinite loop.
     */
    #endregion

    /// <summary>
    /// Attached property bridging <see cref="PasswordBox.Password"/> (not bindable, by
    /// WPF design) to an ordinary bindable string, via <see cref="BoundPasswordProperty"/>.
    /// </summary>
    public static class PasswordBoxBehavior
    {
        #region Attached Properties
        /// <summary>
        /// The bindable string kept in sync with a <see cref="PasswordBox"/>'s actual
        /// <see cref="PasswordBox.Password"/>.
        /// </summary>
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxBehavior),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        // Re-entrancy guard, set while THIS class is the one writing to PasswordBox.Password
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxBehavior), new PropertyMetadata(false));
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the value of <see cref="BoundPasswordProperty"/> on <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The element to read from.</param>
        /// <returns>The current bound password value.</returns>
        public static string GetBoundPassword(DependencyObject target) => (string)target.GetValue(BoundPasswordProperty);

        /// <summary>
        /// Sets the value of <see cref="BoundPasswordProperty"/> on <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The element to write to.</param>
        /// <param name="value">The value to set.</param>
        public static void SetBoundPassword(DependencyObject target, string value) => target.SetValue(BoundPasswordProperty, value);
        #endregion

        #region Private Methods
        // Set once, when BoundPassword is first attached (or its bound source changes)
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox passwordBox)) return;

            passwordBox.PasswordChanged -= HandlePasswordChanged;

            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = e.NewValue as string ?? string.Empty;
            }

            passwordBox.PasswordChanged += HandlePasswordChanged;
        }

        // Fired whenever the user types into the PasswordBox; pushes the new value back
        // into BoundPassword (and, via the TwoWay binding, the bound view model property)
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;

            SetIsUpdating(passwordBox, true);
            SetBoundPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }

        private static void SetIsUpdating(DependencyObject target, bool value) => target.SetValue(IsUpdatingProperty, value);

        private static bool GetIsUpdating(DependencyObject target) => (bool)target.GetValue(IsUpdatingProperty);
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
