using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace ArduinoCompTempMonitor
{
    public partial class Form1 : Form
    {
        private bool _enabled;

        private ArduinoDisplayManager _dispManager;
        private StartupManager _startupManager;

        public Form1()
        {
            InitializeComponent();

            _enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _dispManager = new ArduinoDisplayManager();
            _startupManager = new StartupManager();

            handleInitStartupCheckbox();
        }

        private void handleInitStartupCheckbox()
        {
            if (_startupManager.isAlreadyStartup())
            {
                chkStartup.Checked = true;
                btnStart_Click(this, null);
            }


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!_enabled)
            {
                enable();
            }
            else
            {
                disable();
            }
        }

        private void enable()
        {
            _enabled = true;

            btnStart.Text = "Stop Arduino communication";

            _dispManager.start(Convert.ToInt32(this.txtInterval.Text), this.txtCOMport.Text);
        }

        private void disable()
        {
            _enabled = false;

            _dispManager.stop();

            btnStart.Text = "Start Arduino communication";
        }

        private void btnProbeWPI_Click(object sender, EventArgs e)
        {
            _dispManager.probeWPI();
            MessageBox.Show("Results shown in console.  (May be executed while running in Visual Studio)");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Disable in case it is still running.
            if (_enabled)
                disable();
        }

        private void chkStartup_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStartup.Checked)
                _startupManager.addToStartup();
            else
                _startupManager.removeFromStartup();

        }
    }
}
