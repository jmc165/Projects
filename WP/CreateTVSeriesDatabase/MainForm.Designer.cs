namespace CreateTVSeriesDatabase
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.personPicturesButton = new System.Windows.Forms.Button();
            this.downloadUpdateButton = new System.Windows.Forms.Button();
            this.genresCheckBox = new System.Windows.Forms.CheckBox();
            this.bannersCheckBox = new System.Windows.Forms.CheckBox();
            this.seriesCheckBox = new System.Windows.Forms.CheckBox();
            this.episodesCheckBox = new System.Windows.Forms.CheckBox();
            this.personsCheckBox = new System.Windows.Forms.CheckBox();
            this.unzipButton = new System.Windows.Forms.Button();
            this.searchAndAddSeriesbutton = new System.Windows.Forms.Button();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.deleteTablesButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.createUpdateDatabaseButton = new System.Windows.Forms.Button();
            this.verboseCheckBox = new System.Windows.Forms.CheckBox();
            this.informationTextBox = new System.Windows.Forms.TextBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // createDatabaseButton
            // 
            this.createDatabaseButton.Location = new System.Drawing.Point(12, 69);
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
            // groupBox
            // 
            this.groupBox.Controls.Add(this.personPicturesButton);
            this.groupBox.Controls.Add(this.downloadUpdateButton);
            this.groupBox.Controls.Add(this.genresCheckBox);
            this.groupBox.Controls.Add(this.bannersCheckBox);
            this.groupBox.Controls.Add(this.seriesCheckBox);
            this.groupBox.Controls.Add(this.episodesCheckBox);
            this.groupBox.Controls.Add(this.personsCheckBox);
            this.groupBox.Controls.Add(this.unzipButton);
            this.groupBox.Controls.Add(this.searchAndAddSeriesbutton);
            this.groupBox.Controls.Add(this.inputTextBox);
            this.groupBox.Controls.Add(this.deleteTablesButton);
            this.groupBox.Controls.Add(this.downloadButton);
            this.groupBox.Controls.Add(this.createUpdateDatabaseButton);
            this.groupBox.Controls.Add(this.verboseCheckBox);
            this.groupBox.Controls.Add(this.informationTextBox);
            this.groupBox.Controls.Add(this.createDatabaseButton);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox.Location = new System.Drawing.Point(0, 304);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(629, 175);
            this.groupBox.TabIndex = 4;
            this.groupBox.TabStop = false;
            // 
            // personPicturesButton
            // 
            this.personPicturesButton.Location = new System.Drawing.Point(129, 103);
            this.personPicturesButton.Name = "personPicturesButton";
            this.personPicturesButton.Size = new System.Drawing.Size(111, 28);
            this.personPicturesButton.TabIndex = 24;
            this.personPicturesButton.Text = "Person Pictures";
            this.personPicturesButton.UseVisualStyleBackColor = true;
            this.personPicturesButton.Click += new System.EventHandler(this.OnPersonPicturesButtonClick);
            // 
            // downloadUpdateButton
            // 
            this.downloadUpdateButton.Location = new System.Drawing.Point(246, 35);
            this.downloadUpdateButton.Name = "downloadUpdateButton";
            this.downloadUpdateButton.Size = new System.Drawing.Size(111, 28);
            this.downloadUpdateButton.TabIndex = 23;
            this.downloadUpdateButton.Text = "Download Update";
            this.downloadUpdateButton.UseVisualStyleBackColor = true;
            this.downloadUpdateButton.Click += new System.EventHandler(this.OnDownloadUpdateButtonClick);
            // 
            // genresCheckBox
            // 
            this.genresCheckBox.AutoSize = true;
            this.genresCheckBox.Checked = true;
            this.genresCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.genresCheckBox.Location = new System.Drawing.Point(363, 12);
            this.genresCheckBox.Name = "genresCheckBox";
            this.genresCheckBox.Size = new System.Drawing.Size(60, 17);
            this.genresCheckBox.TabIndex = 22;
            this.genresCheckBox.Text = "Genres";
            this.genresCheckBox.UseVisualStyleBackColor = true;
            // 
            // bannersCheckBox
            // 
            this.bannersCheckBox.AutoSize = true;
            this.bannersCheckBox.Checked = true;
            this.bannersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.bannersCheckBox.Location = new System.Drawing.Point(292, 12);
            this.bannersCheckBox.Name = "bannersCheckBox";
            this.bannersCheckBox.Size = new System.Drawing.Size(65, 17);
            this.bannersCheckBox.TabIndex = 20;
            this.bannersCheckBox.Text = "Banners";
            this.bannersCheckBox.UseVisualStyleBackColor = true;
            // 
            // seriesCheckBox
            // 
            this.seriesCheckBox.AutoSize = true;
            this.seriesCheckBox.Checked = true;
            this.seriesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.seriesCheckBox.Location = new System.Drawing.Point(83, 12);
            this.seriesCheckBox.Name = "seriesCheckBox";
            this.seriesCheckBox.Size = new System.Drawing.Size(55, 17);
            this.seriesCheckBox.TabIndex = 19;
            this.seriesCheckBox.Text = "Series";
            this.seriesCheckBox.UseVisualStyleBackColor = true;
            // 
            // episodesCheckBox
            // 
            this.episodesCheckBox.AutoSize = true;
            this.episodesCheckBox.Checked = true;
            this.episodesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.episodesCheckBox.Location = new System.Drawing.Point(144, 12);
            this.episodesCheckBox.Name = "episodesCheckBox";
            this.episodesCheckBox.Size = new System.Drawing.Size(69, 17);
            this.episodesCheckBox.TabIndex = 18;
            this.episodesCheckBox.Text = "Episodes";
            this.episodesCheckBox.UseVisualStyleBackColor = true;
            // 
            // personsCheckBox
            // 
            this.personsCheckBox.AutoSize = true;
            this.personsCheckBox.Checked = true;
            this.personsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.personsCheckBox.Location = new System.Drawing.Point(219, 12);
            this.personsCheckBox.Name = "personsCheckBox";
            this.personsCheckBox.Size = new System.Drawing.Size(64, 17);
            this.personsCheckBox.TabIndex = 17;
            this.personsCheckBox.Text = "Persons";
            this.personsCheckBox.UseVisualStyleBackColor = true;
            // 
            // unzipButton
            // 
            this.unzipButton.Location = new System.Drawing.Point(129, 35);
            this.unzipButton.Name = "unzipButton";
            this.unzipButton.Size = new System.Drawing.Size(111, 28);
            this.unzipButton.TabIndex = 16;
            this.unzipButton.Text = "Unzip";
            this.unzipButton.UseVisualStyleBackColor = true;
            this.unzipButton.Click += new System.EventHandler(this.OnUnzipButtonClick);
            // 
            // searchAndAddSeriesbutton
            // 
            this.searchAndAddSeriesbutton.Location = new System.Drawing.Point(12, 103);
            this.searchAndAddSeriesbutton.Name = "searchAndAddSeriesbutton";
            this.searchAndAddSeriesbutton.Size = new System.Drawing.Size(111, 28);
            this.searchAndAddSeriesbutton.TabIndex = 15;
            this.searchAndAddSeriesbutton.Text = "Search/Update";
            this.searchAndAddSeriesbutton.UseVisualStyleBackColor = true;
            this.searchAndAddSeriesbutton.Click += new System.EventHandler(this.OnSearchUpdateSeriesClick);
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(347, 143);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(270, 20);
            this.inputTextBox.TabIndex = 14;
            // 
            // deleteTablesButton
            // 
            this.deleteTablesButton.Location = new System.Drawing.Point(363, 35);
            this.deleteTablesButton.Name = "deleteTablesButton";
            this.deleteTablesButton.Size = new System.Drawing.Size(111, 28);
            this.deleteTablesButton.TabIndex = 12;
            this.deleteTablesButton.Text = "Delete Tables";
            this.deleteTablesButton.UseVisualStyleBackColor = true;
            this.deleteTablesButton.Click += new System.EventHandler(this.OnDeleteTablesButtonClick);
            // 
            // downloadButton
            // 
            this.downloadButton.Location = new System.Drawing.Point(12, 35);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(111, 28);
            this.downloadButton.TabIndex = 11;
            this.downloadButton.Text = "Download Zip";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.OnDownloadButtonClick);
            // 
            // createUpdateDatabaseButton
            // 
            this.createUpdateDatabaseButton.Location = new System.Drawing.Point(129, 69);
            this.createUpdateDatabaseButton.Name = "createUpdateDatabaseButton";
            this.createUpdateDatabaseButton.Size = new System.Drawing.Size(111, 28);
            this.createUpdateDatabaseButton.TabIndex = 7;
            this.createUpdateDatabaseButton.Text = "Update Database";
            this.createUpdateDatabaseButton.UseVisualStyleBackColor = true;
            this.createUpdateDatabaseButton.Click += new System.EventHandler(this.OnCreateUpdateDatabaseButtonClick);
            // 
            // verboseCheckBox
            // 
            this.verboseCheckBox.AutoSize = true;
            this.verboseCheckBox.Location = new System.Drawing.Point(12, 12);
            this.verboseCheckBox.Name = "verboseCheckBox";
            this.verboseCheckBox.Size = new System.Drawing.Size(65, 17);
            this.verboseCheckBox.TabIndex = 6;
            this.verboseCheckBox.Text = "Verbose";
            this.verboseCheckBox.UseVisualStyleBackColor = true;
            // 
            // informationTextBox
            // 
            this.informationTextBox.Location = new System.Drawing.Point(12, 143);
            this.informationTextBox.Name = "informationTextBox";
            this.informationTextBox.ReadOnly = true;
            this.informationTextBox.Size = new System.Drawing.Size(329, 20);
            this.informationTextBox.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 478);
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
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TextBox informationTextBox;
        private System.Windows.Forms.CheckBox verboseCheckBox;
        private System.Windows.Forms.Button createUpdateDatabaseButton;
        private System.Windows.Forms.Button downloadButton;
        private System.Windows.Forms.Button deleteTablesButton;
        private System.Windows.Forms.Button searchAndAddSeriesbutton;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button unzipButton;
        private System.Windows.Forms.CheckBox genresCheckBox;
        private System.Windows.Forms.CheckBox bannersCheckBox;
        private System.Windows.Forms.CheckBox seriesCheckBox;
        private System.Windows.Forms.CheckBox episodesCheckBox;
        private System.Windows.Forms.CheckBox personsCheckBox;
        private System.Windows.Forms.Button downloadUpdateButton;
        private System.Windows.Forms.Button personPicturesButton;
    }
}

