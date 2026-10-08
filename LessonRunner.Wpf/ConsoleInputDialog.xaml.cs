#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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
using System.Windows.Input;
#endregion

namespace LessonRunner.Wpf;

#region Supporting Data Types
/// <summary>
/// Specifies the interaction mode used by an input dialog.
/// </summary>
/// <remarks>Use Pause to wait for user acknowledgment without collecting text. Use ReadLine to accept a full line
/// of text input.</remarks>
public enum InputDialogMode { Pause, ReadLine }
#endregion

/// <summary>
/// Represents a modal dialog window that displays console interaction prompts and either collects a line of input or
/// waits for user confirmation to continue.
/// </summary>
/// <remarks>Use <see cref="InputDialogMode.ReadLine"/> to prompt for text input and <see
/// cref="InputDialogMode.Pause"/> to present a continue action without an input field.</remarks>
public partial class ConsoleInputDialog : Window
{
    #region Properties
    /// <summary>
    /// Gets the text entered in the input box.
    /// </summary>
    public string Value => InputBox.Text;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleInputDialog"/> class with the specified prompt text, owner
    /// window, and input mode.
    /// </summary>
    /// <remarks>In <see cref="InputDialogMode.Pause"/>, the input field is hidden and the action button
    /// continues execution. In <see cref="InputDialogMode.ReadLine"/>, the input field is shown and focused for text
    /// entry.</remarks>
    /// <param name="prompt">Prompt text shown to the user. In pause mode, it is displayed as pause context; in read-line mode, it is
    /// displayed as input context.</param>
    /// <param name="owner">Window that owns the dialog.</param>
    /// <param name="mode">Dialog behavior mode that determines whether the dialog waits for continuation or accepts a line of input.</param>
    public ConsoleInputDialog(string prompt, Window owner, InputDialogMode mode = InputDialogMode.ReadLine)
    {
        InitializeComponent();
        Owner = owner;

        if (mode == InputDialogMode.Pause)
        {
            // Pause mode: just show what the program printed and offer Continue.
            // No input field needed.
            IconText.Text      = "⏸";
            PromptText.Text    = string.IsNullOrWhiteSpace(prompt)
                ? "The program has paused. Click Continue to proceed."
                : $"The program has paused:\n\n\"{prompt}\"\n\nClick Continue to proceed.";
            ActionButton.Content = "Continue";
            InputBorder.Visibility = Visibility.Collapsed;
        }
        else
        {
            // ReadLine mode: show what the program is asking for and accept input.
            IconText.Text      = "⌨";
            PromptText.Text    = string.IsNullOrWhiteSpace(prompt)
                ? "The program is waiting for input:"
                : $"The program is asking:\n\n\"{prompt}\"";
            ActionButton.Content = "OK";
            InputBorder.Visibility = Visibility.Visible;
            InputBox.Focus();
        }
    }
    #endregion

    #region Event Handlers
    //Handles the OK action by setting the dialog result to <see langword="true"/>.The source of the event.The routed event data.
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    // Handles the Cancel action by setting the dialog result to <see langword="false"/>.The source of the event.The routed event data.
    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DialogResult = true;
            e.Handled    = true;
        }
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
