using HRPortal.ApiAdapter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace HRPortal.HRMImport.App
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            HRMApiAdapter.GetHostUri = GetHostUri;
        }

        public static string GetHostUri()
        {
            return ConfigurationManager.AppSettings["HRMApiUrl"];
        }

        private async void btnRunImport_Click(object sender, EventArgs e)
        {
            btnRunImport.Enabled = false;

            ImportTask _task = new ImportTask();
            _task.OnMessage += ShowMessage;
            await _task.Run();

            MessageBox.Show("Import completed");
            btnRunImport.Enabled = true;
        }

        private void ShowMessage(string message)
        {
            txtMessage.Text += message + Environment.NewLine;
            if (txtMessage.TextLength > 8000)
            {
                txtMessage.Text = txtMessage.Text.Remove(0, 2000);
                txtMessage.Text = txtMessage.Text.Remove(0, txtMessage.Text.IndexOf(Environment.NewLine) + 2);
            }
            txtMessage.SelectionStart = txtMessage.TextLength;
            txtMessage.ScrollToCaret();
        }
    }
}
