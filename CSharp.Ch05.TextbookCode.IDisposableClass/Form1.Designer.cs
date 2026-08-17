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

namespace CSharp.Ch05.TextbookCode.IDisposableClass
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
            this.collectGarbageButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.createAndDisposeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // collectGarbageButton
            // 
            this.collectGarbageButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.collectGarbageButton.Location = new System.Drawing.Point(247, 33);
            this.collectGarbageButton.Margin = new System.Windows.Forms.Padding(4);
            this.collectGarbageButton.Name = "collectGarbageButton";
            this.collectGarbageButton.Size = new System.Drawing.Size(100, 52);
            this.collectGarbageButton.TabIndex = 8;
            this.collectGarbageButton.Text = "Collect Garbage";
            this.collectGarbageButton.UseVisualStyleBackColor = true;
            this.collectGarbageButton.Click += new System.EventHandler(this.collectGarbageButton_Click);
            // 
            // createButton
            // 
            this.createButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.createButton.Location = new System.Drawing.Point(139, 33);
            this.createButton.Margin = new System.Windows.Forms.Padding(4);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(100, 52);
            this.createButton.TabIndex = 7;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // createAndDisposeButton
            // 
            this.createAndDisposeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.createAndDisposeButton.Location = new System.Drawing.Point(31, 33);
            this.createAndDisposeButton.Margin = new System.Windows.Forms.Padding(4);
            this.createAndDisposeButton.Name = "createAndDisposeButton";
            this.createAndDisposeButton.Size = new System.Drawing.Size(100, 52);
            this.createAndDisposeButton.TabIndex = 6;
            this.createAndDisposeButton.Text = "Create && Dispose";
            this.createAndDisposeButton.UseVisualStyleBackColor = true;
            this.createAndDisposeButton.Click += new System.EventHandler(this.createAndDisposeButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 118);
            this.Controls.Add(this.collectGarbageButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.createAndDisposeButton);
            this.Name = "Form1";
            this.Text = "IDisposableClass";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button collectGarbageButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button createAndDisposeButton;
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
