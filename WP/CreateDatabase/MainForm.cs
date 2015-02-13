using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CreateDatabase
{
    public partial class MainForm : Form
    {
        CDB mcdb = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void OnCreateDatabaseButtonClick(object sender, EventArgs e)
        {
            mcdb = new CDB(
                this.richTextBox,
                this.progressBar,
                this.clearCacheCheckBox.Checked,
                this.deleteDatabaseCheckBox.Checked,
                this.countryNameTextBox.Text);
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (mcdb != null)
                mcdb.Close();
        }

    }
}
