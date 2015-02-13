namespace CreateDatabase
{
    partial class MainForm
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
            this.createDatabaseButton = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.clearCacheCheckBox = new System.Windows.Forms.CheckBox();
            this.deleteDatabaseCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.countryNameTextBox = new System.Windows.Forms.TextBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // createDatabaseButton
            // 
            this.createDatabaseButton.Location = new System.Drawing.Point(512, 12);
            this.createDatabaseButton.Name = "createDatabaseButton";
            this.createDatabaseButton.Size = new System.Drawing.Size(111, 28);
            this.createDatabaseButton.TabIndex = 0;
            this.createDatabaseButton.Text = "Create Database";
            this.createDatabaseButton.UseVisualStyleBackColor = true;
            this.createDatabaseButton.Click += new System.EventHandler(this.OnCreateDatabaseButtonClick);
            // 
            // richTextBox
            // 
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(629, 304);
            this.richTextBox.TabIndex = 1;
            this.richTextBox.Text = "";
            // 
            // clearCacheCheckBox
            // 
            this.clearCacheCheckBox.AutoSize = true;
            this.clearCacheCheckBox.Location = new System.Drawing.Point(12, 42);
            this.clearCacheCheckBox.Name = "clearCacheCheckBox";
            this.clearCacheCheckBox.Size = new System.Drawing.Size(84, 17);
            this.clearCacheCheckBox.TabIndex = 2;
            this.clearCacheCheckBox.Text = "Clear Cache";
            this.clearCacheCheckBox.UseVisualStyleBackColor = true;
            // 
            // deleteDatabaseCheckBox
            // 
            this.deleteDatabaseCheckBox.AutoSize = true;
            this.deleteDatabaseCheckBox.Checked = true;
            this.deleteDatabaseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deleteDatabaseCheckBox.Location = new System.Drawing.Point(12, 19);
            this.deleteDatabaseCheckBox.Name = "deleteDatabaseCheckBox";
            this.deleteDatabaseCheckBox.Size = new System.Drawing.Size(106, 17);
            this.deleteDatabaseCheckBox.TabIndex = 3;
            this.deleteDatabaseCheckBox.Text = "Delete Database";
            this.deleteDatabaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.countryNameTextBox);
            this.groupBox.Controls.Add(this.deleteDatabaseCheckBox);
            this.groupBox.Controls.Add(this.clearCacheCheckBox);
            this.groupBox.Controls.Add(this.createDatabaseButton);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox.Location = new System.Drawing.Point(0, 304);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(629, 100);
            this.groupBox.TabIndex = 4;
            this.groupBox.TabStop = false;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressBar.Location = new System.Drawing.Point(0, 404);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(629, 23);
            this.progressBar.TabIndex = 5;
            // 
            // countryNameTextBox
            // 
            this.countryNameTextBox.Location = new System.Drawing.Point(12, 65);
            this.countryNameTextBox.Name = "countryNameTextBox";
            this.countryNameTextBox.Size = new System.Drawing.Size(100, 20);
            this.countryNameTextBox.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 436);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.richTextBox);
            this.Name = "MainForm";
            this.Text = "Create Database";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnMainFormClosing);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button createDatabaseButton;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.CheckBox clearCacheCheckBox;
        private System.Windows.Forms.CheckBox deleteDatabaseCheckBox;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox countryNameTextBox;
    }
}

