using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using IronPdf;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronPdf.PrintDoc;
using IronPdf.Pdfium;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Printing;
using System.IO;
using System.Runtime.InteropServices;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Reflection;

namespace Pos_Print
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Button printButton;
        public static Font printFont;
        public static StreamReader streamToPrint;
        private static Stream IOStream;
        public static PdfReader reader;
        public Form1()
        {
            InitializeComponent();
        }
        public static class myPrinters

        {

            [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]

            public static extern bool SetDefaultPrinter(string Name);

        }

        private void OpenPDF_Click(object sender, EventArgs e)
        {
            
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

            //IronPdf.ChromePdfRenderer renderer = new IronPdf.ChromePdfRenderer();
            //IronPdf.PdfDocument doc = renderer.RenderUrlAsPdf("https://erp.cloudmedia.vn/web?db=cloudmedia&token=e5mJmoBL0XwCtejfMfwN#action=507&model=project.project&view_type=kanban&cids=1&menu_id=339");
            //doc.GetPrintDocument().PrinterSettings.PrinterName = "ZJ-58";
            //doc.PrintToFile("E:\\iron1.pdf");
            string tempFile = @"E:\bill_letter.pdf";
            try
            {
                ProcessStartInfo gsProcessInfo;
                Process gsProcess;
                string printerName = "OneNote for Windows 10";

                gsProcessInfo = new ProcessStartInfo();
                gsProcessInfo.Verb = "PrintTo";
                gsProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
                gsProcessInfo.FileName = tempFile.Trim();
                gsProcessInfo.Arguments = "\"" + printerName + "\"";
                gsProcess = Process.Start(gsProcessInfo);

                gsProcess.EnableRaisingEvents = true;

                gsProcess.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            //PrintDocumentMethod.Check_Printer();


        }
        public class PrintDocumentMethod
        {
            public static void Check_Printer()
            {
                using (LocalPrintServer printServer = new LocalPrintServer())
                {
                    PrintQueueCollection printQueuesOnLocalServer = printServer.GetPrintQueues();
                    foreach (PrintQueue pq in printQueuesOnLocalServer)
                    {
                        if (pq.Name == "OneNote for Windows 10")
                        {
                            Printing(pq.Name);
                        }
                        Console.WriteLine("Tên máy in: " + pq.Name);
                        Console.WriteLine("Địa chỉ IP: " + pq.QueuePort.Name);
                        Console.WriteLine("Trạng thái: " + pq.QueueStatus);
                        Console.WriteLine("Số lượng trang đã in: " + pq.NumberOfJobs);
                    }
                }

            }

            public static void Printing(string printer)
            {
                try
                {
                    streamToPrint = new StreamReader("E:\\bill_letter.pdf");
                    try
                    {
                        printFont = new Font("Arial", 10);
                        PrintDocument pd = new PrintDocument();
                        pd.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
                        // Specify the printer to use.
                        pd.PrinterSettings.PrinterName = printer;

                        if (pd.PrinterSettings.IsValid)
                        {
                            pd.Print();
                        }
                        else
                        {
                            MessageBox.Show("Printer is invalid.");
                        }
                    }
                    finally
                    {
                        streamToPrint.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        

        public static void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = e.MarginBounds.Height /
               printFont.GetHeight(e.Graphics);

            // Print each line of the file.
            
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count *
                printFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(line, printFont, Brushes.Black,
                    leftMargin / 10, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
                e.HasMorePages = true;
            else
                e.HasMorePages = false;

            //e.Graphics.DrawImage(Image.FromStream(IOStream, true, false), e.Graphics.VisibleClipBounds);
            //e.HasMorePages = false;
        }
    }
}
