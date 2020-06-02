using PEPlugin;
using PEPlugin.Pmx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomaticUVUnwrapping
{
    public partial class CtrlForm : Form
    {
        IPERunArgs args;
        IPXPmx pmx;

        public CtrlForm(IPERunArgs input)
        {
            InitializeComponent();
            args = input;
            Format();
        }

        public void Format()
        {
            pmx = args.Host.Connector.Pmx.GetCurrentState();
        }

        private void CtrlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Visible = false;
            }
        }
    }
}