using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using OSHFT_Q_R.Market;

namespace OSHFT_Q_R
{

    public partial class OSHFT_Q_RMain : Form
    {
        DataManager dm;
        TermManager tmgr;

        public OSHFT_Q_RMain()
        {
            InitializeComponent();
        }

        // **********************************************************************

        public static void ShowMessage(string text)
        {
            MessageBox.Show(text, cfg.FullProgName);
        }

        public void LogToScreeen(string text)
        {
            tbxLogs.AppendText(text + Environment.NewLine);
        }

        private void OSHFT_Q_RMain_Shown(object sender, EventArgs e)
        {
            dm = new DataManager();
            tmgr = new TermManager(dm);
            MarketProvider.SetReceiver(dm);
            ExchangeManager.SetDataManager(dm);
            ExchangeManager.SetTermManager(tmgr);
            ExchangeManager.SetMainForm(this);

            MarketProvider.Activate();
            ExchangeManager.Activate();

            tmgr.Connect();
        }

        private void OSHFT_Q_RMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            MarketProvider.Deactivate();
            ExchangeManager.Deactivate();

            cfg.SaveUserConfig(cfg.UserCfgFile);

            tmgr.Disconnect();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void опрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout about = new frmAbout();
            about.Show();
        }
    }
}
