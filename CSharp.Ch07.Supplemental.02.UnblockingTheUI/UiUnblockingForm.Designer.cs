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

namespace CSharp.Ch07.Supplemental._02.UnblockingTheUI
{
    partial class UiUnblockingForm
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
            this.BtnBlock = new System.Windows.Forms.Button();
            this.BtnUnblock = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnBlock
            // 
            this.BtnBlock.Location = new System.Drawing.Point(74, 63);
            this.BtnBlock.Name = "BtnBlock";
            this.BtnBlock.Size = new System.Drawing.Size(361, 34);
            this.BtnBlock.TabIndex = 0;
            this.BtnBlock.Text = "Run Process Blocking the UI Thread";
            this.BtnBlock.UseVisualStyleBackColor = true;
            this.BtnBlock.Click += new System.EventHandler(this.BtnBlock_Click);
            // 
            // BtnUnblock
            // 
            this.BtnUnblock.Location = new System.Drawing.Point(74, 139);
            this.BtnUnblock.Name = "BtnUnblock";
            this.BtnUnblock.Size = new System.Drawing.Size(361, 34);
            this.BtnUnblock.TabIndex = 1;
            this.BtnUnblock.Text = "Run Process Unblocking the UI Thread";
            this.BtnUnblock.UseVisualStyleBackColor = true;
            this.BtnUnblock.Click += new System.EventHandler(this.BtnUnblock_Click);
            // 
            // UiUnblockingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 275);
            this.Controls.Add(this.BtnUnblock);
            this.Controls.Add(this.BtnBlock);
            this.Name = "UiUnblockingForm";
            this.Text = "UI Thread Blocking Example";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnBlock;
        private System.Windows.Forms.Button BtnUnblock;
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
