using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CreateTVSeriesDatabase
{
    public partial class MainForm : Form
    {
        CDB mcdb = null;

        public MainForm()
        {
            InitializeComponent();
#if DEBUG
            this.Text = "Create TV Series Database (DEBUG)";
#else
            this.Text = "Create TV Series Database (Release)";
#endif
        }

        private void OnCreateDatabaseButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "CreateDatabase");
        }

        private void OnCreateUpdateDatabaseButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "CreateUpdateDatabase");
        }

        private void OnDeleteTablesButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "DeleteTables");
        }

        private void OnDownloadButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "Download");
        }

        private void OnUnzipButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "Unzip");
        }

        private void OnDownloadUpdateButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.informationTextBox,
                this.inputTextBox,
                new bool[] {
                this.verboseCheckBox.Checked, 
                seriesCheckBox.Checked,
                this.episodesCheckBox.Checked,
                this.personsCheckBox.Checked,
                this.bannersCheckBox.Checked,
                this.genresCheckBox.Checked },
                "DownloadUpdate");
        }

        private void OnPersonPicturesButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                     this.richTextBox,
                     this.informationTextBox,
                     this.inputTextBox,
                new bool[] {
                    this.verboseCheckBox.Checked, 
                    seriesCheckBox.Checked,
                    this.episodesCheckBox.Checked,
                    this.personsCheckBox.Checked,
                    this.bannersCheckBox.Checked,
                    this.genresCheckBox.Checked },
                     "PersonPictures");
        }

        private void OnSearchUpdateSeriesClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                      this.richTextBox,
                      this.informationTextBox,
                      this.inputTextBox,
                 new bool[] {
                    this.verboseCheckBox.Checked, 
                    seriesCheckBox.Checked,
                    this.episodesCheckBox.Checked,
                    this.personsCheckBox.Checked,
                    this.bannersCheckBox.Checked,
                    this.genresCheckBox.Checked },
                      "SearchAndUpdate");

        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (mcdb != null)
                mcdb.Close();
        }

   
    }
}
