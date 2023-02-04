using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using iTextSharp.text.pdf.qrcode;
using System.Web;

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
            //http://127.0.0.1:3000/cgi-bin/eposDisp/service.cgi/
            //http://127.0.0.1:3000/cgi-bin/epos/service.cgi/
            listener.Prefixes.Add(txtHost.Text+":"+txtPort.Text+"/"+txtUrl.Text);
            WriteLog("Start listen");
            listener.Start();
            while (!isDisposited)
            {

                HttpListenerContext context = listener.GetContext();
                HttpListenerTimeoutManager manager = listener.TimeoutManager;
                manager.IdleConnection = TimeSpan.FromMinutes(1);
                manager.HeaderWait = TimeSpan.FromMinutes(1);
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
                        Regex reg = new Regex("<image width=\"([0-9]*)\" height=\"([0-9]*)\">([^<]*)</image>");
                        if (reg.IsMatch(data))
                        {
                            var math = reg.Match(data);
                            int width = int.Parse(math.Groups[1].Value);
                            int height = int.Parse(math.Groups[2].Value);
                            string base64 = math.Groups[3].Value;
                            ConvertPrintDataImage(base64, width, height);
                           
                            //WriteLog(base64);
                        }
                        string pathImage = Application.StartupPath + @"\assets\image\image.png";
                        string pathPDF = Application.StartupPath + @"\assets\pdf\Bill.pdf";
                        ImagesToPdf(pathImage, pathPDF);
                        PrintDocument pdoc = new PrintDocument();
                        pdoc.DefaultPageSettings.PrinterSettings.PrinterName = "ZJ-58";
                        pdoc.DefaultPageSettings.Landscape = true;
                        
                        Print(pdoc.PrinterSettings.PrinterName, pathPDF);
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

        public void ConvertPrintDataImage(string base64String, int width, int height)
        {
            byte[] data = Convert.FromBase64String(base64String);
            //int width = 100; // set the width of the bitmap
            //int height = 100; // set the height of the bitmap
            int stride = width * 4; // calculate the stride

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr scan0 = bitmapData.Scan0;
            unsafe
            {
                byte* p = (byte*)scan0.ToPointer();

                int index = 0;
                for (int y = 0; y < height; y++)
                {

                    for (int x = 0; x < width; x++)
                    {
                        try
                        {
                            byte v = data[index / 2];
                            if (v > 180) v = 255;
                            if (v < 120) v = 0;
                            p[0] = v;
                            p[1] = v;
                            p[2] = v;
                            p[3] = 255;
                        }
                        catch (Exception ex)
                        {

                        }
                        index++;
                        p += 4;
                    }
                    p += bitmapData.Stride - width * 4;
                }
            }
            bitmap.UnlockBits(bitmapData);

            bitmap.Save(Application.StartupPath + @"\assets\image\image.png", ImageFormat.Png);
        }
        public void ImagesToPdf(string imagepaths, string pdfpath)
        {
            iTextSharp.text.Rectangle pageSize = null;

            using (var srcImage = new Bitmap(imagepaths.ToString()))
            {
                pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
            }

            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
                document.Open();
                var image = iTextSharp.text.Image.GetInstance(imagepaths.ToString());
                document.Add(image);
                document.Close();

                File.WriteAllBytes(pdfpath, ms.ToArray());
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
                gsProcessInfo.Verb = "Print";
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
        private void btnStop_Click(object sender, EventArgs e)
        {
            isDisposited = true;
            WriteLog("Stopping...");
        }

       
    }
}
