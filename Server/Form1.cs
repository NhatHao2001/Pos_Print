using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    public partial class Form1 : Form
    {
        SimpleTcpServer server;
        bool isDisposited = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(StartHttpServer));
            t.Start();
        }
        private void Server_DataReceived (object sender, SimpleTCP.Message e)
        {
            txtMessageBox.Invoke((MethodInvoker)delegate ()
            {

                txtMessageBox.Text += System.Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + e.MessageString;
                
                e.ReplyLine(string.Format(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt")+" Client: {0}", e.MessageString));
                
            });
        }

        private void StartHttpServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:3000/cgi-bin/epos/service.cgi/");
            WriteLog("Start listen");
            listener.Start();
            while (!isDisposited)
            {

                HttpListenerContext context = listener.GetContext();

                //Can create a thread here to process request parallel
                ProcessRequest(context);

            }
            listener.Stop();
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            string devid = request.QueryString["devid"];
            WriteLog(devid);
            if (request.HasEntityBody)
            {
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string data = reader.ReadToEnd();
                    // Process the received string data
                    
                    WriteLog(data);
                    XmlDocument xmlDoc = new XmlDocument();
                    if (!string.IsNullOrEmpty(data) && data.TrimStart().StartsWith("<?xml"))
                    {
                        xmlDoc.LoadXml(data);
                        var base64 = xmlDoc.InnerText.ToString();
                        string FolderName = @"E:/";
                        string FileName = FolderName + "Bill.pdf";
                        byte[] byteArray = Convert.FromBase64String(base64);
                        SaveByteArrayToFileWithFileStream(byteArray, FileName);
                        PrintDocument pdoc = new PrintDocument();

                        pdoc.DefaultPageSettings.PrinterSettings.PrinterName = "ZJ-58";
                        pdoc.DefaultPageSettings.Landscape = true;
                        pdoc.DefaultPageSettings.PaperSize = new PaperSize("custom", 104, 140);
                        Print(pdoc.PrinterSettings.PrinterName, FileName);
                    }

                }
            }
            string responseString = "<response success=\"1\"></response>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.OK;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public void SaveByteArrayToFileWithFileStream(byte[] data, string filePath)
        {
            using (var filestream = File.OpenWrite(filePath))
            {
                BinaryWriter bw = new BinaryWriter(filestream);
                bw.Write(data, 0, (int)data.Length);
                bw.Close();
            }
        }
        public void Print(string printerName, string fileName)
        {
            //fileName = @"E:\bill_letter.pdf";
            try
            {
                ProcessStartInfo gsProcessInfo;
                Process gsProcess;
                //printerName = "OneNote for Windows 10";

                gsProcessInfo = new ProcessStartInfo();
                gsProcessInfo.Verb = "PrintTo";
                gsProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
                gsProcessInfo.FileName = fileName.Trim();
                gsProcessInfo.Arguments = "\"" + printerName + "\"";

                gsProcess = Process.Start(gsProcessInfo);

                gsProcess.EnableRaisingEvents = true;

                gsProcess.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void WriteLog(string message)
        {
            txtMessageBox.Invoke((MethodInvoker)delegate ()
            {

                txtMessageBox.Text += System.Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + message;

            });
        }

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    server = new SimpleTcpServer();
        //    server.Delimiter = 0x13;
        //    server.StringEncoder = Encoding.UTF8;
        //    server.DataReceived += Server_DataReceived;
        //}

        private void btnStop_Click(object sender, EventArgs e)
        {
            isDisposited = true;
            WriteLog("Stopping...");
        }
    }
}
