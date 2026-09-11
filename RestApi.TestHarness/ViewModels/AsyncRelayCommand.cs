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
using System.Threading.Tasks;
using System.Windows.Input;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own AsyncRelayCommand, purely
     * generic MVVM infrastructure with no Unity API/REST API dependency at all. If
     * anything, this project leans on it MORE than Unity.TestHarness did: every one of
     * this project's own OnBase calls is genuinely async (RestApi.01-04 are async/await
     * throughout, unlike Unity API's synchronous SDK), so nearly every command in this
     * app is an AsyncRelayCommand rather than a plain RelayCommand.
     */
    #endregion

    /// <summary>
    /// An <see cref="ICommand"/> implementation for asynchronous work, delegating to a
    /// caller-supplied <see cref="Func{Object, Task}"/> rather than a plain
    /// <see cref="Action{Object}"/>, and automatically disabling itself while that work
    /// is in progress.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the AsyncRelayCommand class
    /// </remarks>
    /// <param name="executeAsync">The asynchronous work to perform on execute.</param>
    /// <param name="canExecute">The predicate to evaluate for CanExecute (optional, defaults to always executable).</param>
    public class AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null) : ICommand
    {
        #region Private Members
        // The asynchronous work to perform on execute
        private readonly Func<object, Task> executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));

        // The predicate to evaluate for CanExecute (optional)
        private readonly Predicate<object> canExecute = canExecute;

        // Whether executeAsync is currently running
        private bool isExecuting;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return !isExecuting && (canExecute == null || canExecute(parameter));
        }

        /// <inheritdoc />
        public async void Execute(object parameter)
        {
            isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await executeAsync(parameter);
            }
            finally
            {
                isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
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
