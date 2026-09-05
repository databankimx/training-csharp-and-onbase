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
using System.Windows.Input;
#endregion

namespace Unity.TestHarness.ViewModels
{
    /// <summary>
    /// A generic <see cref="ICommand"/> implementation, delegating <see cref="Execute"/>/
    /// <see cref="CanExecute"/> to caller-supplied delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private Members
        // The action to invoke on execute
        private readonly Action<object> execute;

        // The predicate to evaluate for CanExecute (optional)
        private readonly Predicate<object> canExecute;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the RelayCommand class
        /// </summary>
        /// <param name="execute">The action to invoke on execute.</param>
        /// <param name="canExecute">The predicate to evaluate for CanExecute (optional, defaults to always executable).</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            execute(parameter);
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
