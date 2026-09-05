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

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: ICommand.Execute is a framework-mandated `void Execute(object)`
     * signature (System.Windows.Input.ICommand), there's no way around SOME method
     * ending in async void here, WPF's own interface requires it. The point of this
     * class isn't eliminating that entirely, it's confining it to exactly one place
     * (Execute() below) rather than letting it leak into every view model method that
     * happens to be wired up to a command. The actual work delegate (executeAsync) is a
     * genuine Func<object, Task>, so a view model's real async method (e.g.
     * ConnectionViewModel.TestServer()) can be written as `private async Task
     * TestServer()`, a normal, awaitable, exception-propagating method, satisfying
     * analyzers like SonarQube's S3168 ("async void should return Task") at the one
     * layer that's actually able to.
     *
     * IsExecuting also makes CanExecute automatically false WHILE the command is
     * running, a real correctness improvement over RelayCommand for anything
     * network-bound: without it, nothing stops a user from clicking "Test Connection"
     * (or any future async Taxonomy/Retrieval/Archiving action) a second time before the
     * first call finishes.
     */
    #endregion

    /// <summary>
    /// An <see cref="ICommand"/> implementation for asynchronous work, delegating to a
    /// caller-supplied <see cref="Func{Object, Task}"/> rather than a plain
    /// <see cref="Action{Object}"/>, and automatically disabling itself while that work
    /// is in progress.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        #region Private Members
        // The asynchronous work to perform on execute
        private readonly Func<object, Task> executeAsync;

        // The predicate to evaluate for CanExecute (optional)
        private readonly Predicate<object> canExecute;

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

        #region Constructors
        /// <summary>
        /// Create a new instance of the AsyncRelayCommand class
        /// </summary>
        /// <param name="executeAsync">The asynchronous work to perform on execute.</param>
        /// <param name="canExecute">The predicate to evaluate for CanExecute (optional, defaults to always executable).</param>
        public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecute = canExecute;
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
