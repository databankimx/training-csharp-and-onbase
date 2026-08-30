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
#endregion

namespace Samples.Wpf
{
    // *Migration Note: this is the ENTIRE code-behind file. No event handlers, no data
    //   access, no query logic, everything that matters lives in MainViewModel.cs (which
    //   knows nothing about WPF at all) and MainWindow.xaml (which knows nothing about EF
    //   Core or SQL at all). This separation is the whole point of MVVM. See
    //   LectureNotes.md.
    /// <summary>
    /// The application's main window. Its DataContext is set to a <see cref="ViewModels.MainViewModel"/>
    /// instance by <see cref="App.OnStartup(StartupEventArgs)"/>; this class itself contains no
    /// application logic, only the standard InitializeComponent() call.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the MainWindow class. This constructor initializes the WPF window and its components.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
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
