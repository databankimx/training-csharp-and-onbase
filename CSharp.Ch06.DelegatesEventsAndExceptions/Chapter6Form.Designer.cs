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

namespace CSharp.Ch06.DelegatesEventsAndExceptions
{
    partial class Chapter6Form
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
            this.BtnAnon = new System.Windows.Forms.Button();
            this.CbTracked = new System.Windows.Forms.CheckBox();
            this.BtnGraphForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnAnon
            // 
            this.BtnAnon.Location = new System.Drawing.Point(48, 35);
            this.BtnAnon.Name = "BtnAnon";
            this.BtnAnon.Size = new System.Drawing.Size(202, 23);
            this.BtnAnon.TabIndex = 0;
            this.BtnAnon.Text = "Click me if you dare!";
            this.BtnAnon.UseVisualStyleBackColor = true;
            // 
            // CbTracked
            // 
            this.CbTracked.AutoSize = true;
            this.CbTracked.Location = new System.Drawing.Point(78, 86);
            this.CbTracked.Name = "CbTracked";
            this.CbTracked.Size = new System.Drawing.Size(92, 21);
            this.CbTracked.TabIndex = 1;
            this.CbTracked.Text = "Check Me";
            this.CbTracked.UseVisualStyleBackColor = true;
            // 
            // BtnGraphForm
            // 
            this.BtnGraphForm.Location = new System.Drawing.Point(153, 123);
            this.BtnGraphForm.Name = "BtnGraphForm";
            this.BtnGraphForm.Size = new System.Drawing.Size(141, 23);
            this.BtnGraphForm.TabIndex = 2;
            this.BtnGraphForm.Text = "Show Graph Form";
            this.BtnGraphForm.UseVisualStyleBackColor = true;
            this.BtnGraphForm.Click += new System.EventHandler(this.BtnGraphForm_Click);
            // 
            // Chapter6Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 170);
            this.Controls.Add(this.BtnGraphForm);
            this.Controls.Add(this.CbTracked);
            this.Controls.Add(this.BtnAnon);
            this.MaximumSize = new System.Drawing.Size(347, 217);
            this.MinimumSize = new System.Drawing.Size(347, 217);
            this.Name = "Chapter6Form";
            this.Text = "Training Example Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnAnon;
        private System.Windows.Forms.CheckBox CbTracked;
        private System.Windows.Forms.Button BtnGraphForm;
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
