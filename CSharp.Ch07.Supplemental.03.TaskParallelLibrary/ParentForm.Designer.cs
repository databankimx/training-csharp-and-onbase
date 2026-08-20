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

namespace CSharp.Ch07.Supplemental._03.TaskParallelLibrary
{
    partial class ParentForm
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
            this.BtnCannot = new System.Windows.Forms.Button();
            this.BtnCan = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LblSource = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnCannot
            // 
            this.BtnCannot.Location = new System.Drawing.Point(59, 70);
            this.BtnCannot.Name = "BtnCannot";
            this.BtnCannot.Size = new System.Drawing.Size(280, 43);
            this.BtnCannot.TabIndex = 0;
            this.BtnCannot.Text = "Run Task that Cannot Update the UI";
            this.BtnCannot.UseVisualStyleBackColor = true;
            this.BtnCannot.Click += new System.EventHandler(this.BtnCannot_Click);
            // 
            // BtnCan
            // 
            this.BtnCan.Location = new System.Drawing.Point(59, 157);
            this.BtnCan.Name = "BtnCan";
            this.BtnCan.Size = new System.Drawing.Size(280, 43);
            this.BtnCan.TabIndex = 1;
            this.BtnCan.Text = "Run Task that Can Update the UI";
            this.BtnCan.UseVisualStyleBackColor = true;
            this.BtnCan.Click += new System.EventHandler(this.BtnCan_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(85, 247);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Click Source:";
            // 
            // LblSource
            // 
            this.LblSource.AutoSize = true;
            this.LblSource.Location = new System.Drawing.Point(208, 247);
            this.LblSource.Name = "LblSource";
            this.LblSource.Size = new System.Drawing.Size(42, 17);
            this.LblSource.TabIndex = 3;
            this.LblSource.Text = "None";
            // 
            // ParentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.LblSource);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnCan);
            this.Controls.Add(this.BtnCannot);
            this.Name = "ParentForm";
            this.Text = "ParentForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnCannot;
        private System.Windows.Forms.Button BtnCan;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LblSource;
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
