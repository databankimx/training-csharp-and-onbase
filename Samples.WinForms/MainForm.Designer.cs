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

namespace Samples.WinForms
{
    partial class MainForm
    {
        #region Fields
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null!;

        private Label lblZipCode = null!;
        private TextBox txtZipCode = null!;
        private Button btnSearch = null!;
        private Label lblError = null!;
        private DataGridView gridResults = null!;
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed; otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support, lays out every control on the form
        /// entirely in C#, no markup language at all. Contrast this against
        /// Samples.Wpf/MainWindow.xaml, which declares the identical shape of UI
        /// (a text input, a search button, an error label, a results grid) in
        /// declarative XAML instead. See LectureNotes.md. Do not modify the contents
        /// of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblZipCode = new Label();
            txtZipCode = new TextBox();
            btnSearch = new Button();
            lblError = new Label();
            gridResults = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)gridResults).BeginInit();
            SuspendLayout();

            // lblZipCode
            lblZipCode.AutoSize = true;
            lblZipCode.Location = new Point(16, 20);
            lblZipCode.Name = "lblZipCode";
            lblZipCode.Size = new Size(58, 15);
            lblZipCode.Text = "Zip Code:";

            // txtZipCode
            txtZipCode.Location = new Point(80, 17);
            txtZipCode.MaxLength = 5;
            txtZipCode.Name = "txtZipCode";
            txtZipCode.Size = new Size(100, 23);
            txtZipCode.Text = "75067";

            // btnSearch
            // *Migration Note: notice the direct Click event subscription below, this is
            //   the idiomatic WinForms pattern, no Command binding, no ViewModel, the
            //   Button itself directly triggers imperative code in MainForm.cs.
            btnSearch.Location = new Point(196, 16);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(80, 25);
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += BtnSearch_Click;

            // lblError
            lblError.AutoSize = true;
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(16, 52);
            lblError.Name = "lblError";
            lblError.Size = new Size(0, 15);

            // gridResults
            gridResults.AllowUserToAddRows = false;
            gridResults.AllowUserToDeleteRows = false;
            gridResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResults.Location = new Point(16, 80);
            gridResults.Name = "gridResults";
            gridResults.ReadOnly = true;
            gridResults.RowHeadersVisible = false;
            gridResults.Size = new Size(608, 360);

            // MainForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 460);
            Controls.Add(gridResults);
            Controls.Add(lblError);
            Controls.Add(btnSearch);
            Controls.Add(txtZipCode);
            Controls.Add(lblZipCode);
            MinimumSize = new Size(500, 350);
            Name = "MainForm";
            Text = "Location Lookup (WinForms)";
            ((System.ComponentModel.ISupportInitialize)gridResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
