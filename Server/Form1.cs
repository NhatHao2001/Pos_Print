using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        SimpleTcpServer server;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            txtMessageBox.Text += "Server starting....";
            System.Net.IPAddress ip = new System.Net.IPAddress(IPAddress.Parse(txtHost.Text).Address);
            server.Start(ip, Convert.ToInt32(txtPort.Text));
        }
        private void Server_DataReceived (object sender, SimpleTCP.Message e)
        {
            txtMessageBox.Invoke((MethodInvoker)delegate ()
            {

                txtMessageBox.Text += System.Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + e.MessageString;
                
                e.ReplyLine(string.Format(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt")+" Client: {0}", e.MessageString));
                
            });
        }

        private void txtHost_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtMessageBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new SimpleTcpServer();
            server.Delimiter = 0x13;
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (server.IsStarted)
            {
                server.Stop();
            }
        }
    }
}
