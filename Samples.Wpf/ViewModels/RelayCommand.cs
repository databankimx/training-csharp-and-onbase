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

namespace Samples.Wpf.ViewModels
{
    /// <summary>
    /// A generic ICommand implementation, standard MVVM boilerplate (WPF itself doesn't
    /// ship one). A View binds a button's Command property directly to a property of this
    /// type on the ViewModel, no Click event handler in code-behind at all, the ViewModel
    /// decides when the command can run (CanExecute) and what happens when it does
    /// (Execute), and WPF's binding engine handles enabling/disabling the button
    /// automatically based on CanExecute.
    /// </summary>
    /// <param name="execute">The action to invoke when the command is executed.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can currently execute. When omitted, the command can always execute.</param>
    public class RelayCommand(Action execute, Func<bool> canExecute = null) : ICommand
    {
        #region Events
        /// <summary>
        /// Handler for the CanExecuteChanged event, which WPF's binding engine listens to in order
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">An optional data value passed by the command source.</param>
        /// <returns>true if no execution predicate is specified or if the execution predicate returns true; otherwise, false.</returns>
        public bool CanExecute(object parameter) => canExecute?.Invoke() ?? true;

        /// <summary>
        /// Invokes the configured execution delegate.
        /// </summary>
        /// <param name="parameter">An optional command parameter. The value is ignored.</param>
        public void Execute(object parameter) => execute();
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
