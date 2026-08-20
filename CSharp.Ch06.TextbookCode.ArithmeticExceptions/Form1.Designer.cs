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

namespace CSharp.Ch06.TextbookCode.ArithmeticExceptions
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.floatDivideBy0Button = new System.Windows.Forms.Button();
            this.floatOverflowButton = new System.Windows.Forms.Button();
            this.integersCheckedButton = new System.Windows.Forms.Button();
            this.integerOverflowButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // floatDivideBy0Button
            // 
            this.floatDivideBy0Button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.floatDivideBy0Button.Location = new System.Drawing.Point(105, 129);
            this.floatDivideBy0Button.Margin = new System.Windows.Forms.Padding(4);
            this.floatDivideBy0Button.Name = "floatDivideBy0Button";
            this.floatDivideBy0Button.Size = new System.Drawing.Size(147, 28);
            this.floatDivideBy0Button.TabIndex = 7;
            this.floatDivideBy0Button.Text = "Float Divide By 0";
            this.floatDivideBy0Button.UseVisualStyleBackColor = true;
            this.floatDivideBy0Button.Click += new System.EventHandler(this.floatDivideBy0Button_Click);
            // 
            // floatOverflowButton
            // 
            this.floatOverflowButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.floatOverflowButton.Location = new System.Drawing.Point(105, 93);
            this.floatOverflowButton.Margin = new System.Windows.Forms.Padding(4);
            this.floatOverflowButton.Name = "floatOverflowButton";
            this.floatOverflowButton.Size = new System.Drawing.Size(147, 28);
            this.floatOverflowButton.TabIndex = 6;
            this.floatOverflowButton.Text = "Float Overflow";
            this.floatOverflowButton.UseVisualStyleBackColor = true;
            this.floatOverflowButton.Click += new System.EventHandler(this.floatOverflowButton_Click);
            // 
            // integersCheckedButton
            // 
            this.integersCheckedButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.integersCheckedButton.Location = new System.Drawing.Point(105, 58);
            this.integersCheckedButton.Margin = new System.Windows.Forms.Padding(4);
            this.integersCheckedButton.Name = "integersCheckedButton";
            this.integersCheckedButton.Size = new System.Drawing.Size(147, 28);
            this.integersCheckedButton.TabIndex = 5;
            this.integersCheckedButton.Text = "Integers Checked";
            this.integersCheckedButton.UseVisualStyleBackColor = true;
            this.integersCheckedButton.Click += new System.EventHandler(this.integersCheckedButton_Click);
            // 
            // integerOverflowButton
            // 
            this.integerOverflowButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.integerOverflowButton.Location = new System.Drawing.Point(105, 22);
            this.integerOverflowButton.Margin = new System.Windows.Forms.Padding(4);
            this.integerOverflowButton.Name = "integerOverflowButton";
            this.integerOverflowButton.Size = new System.Drawing.Size(147, 28);
            this.integerOverflowButton.TabIndex = 4;
            this.integerOverflowButton.Text = "Integer Overflow";
            this.integerOverflowButton.UseVisualStyleBackColor = true;
            this.integerOverflowButton.Click += new System.EventHandler(this.integerOverflowButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 178);
            this.Controls.Add(this.floatDivideBy0Button);
            this.Controls.Add(this.floatOverflowButton);
            this.Controls.Add(this.integersCheckedButton);
            this.Controls.Add(this.integerOverflowButton);
            this.Name = "Form1";
            this.Text = "ArithmeticExceptions";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button floatDivideBy0Button;
        private System.Windows.Forms.Button floatOverflowButton;
        private System.Windows.Forms.Button integersCheckedButton;
        private System.Windows.Forms.Button integerOverflowButton;
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
