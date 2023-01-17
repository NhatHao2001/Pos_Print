using Pos_Print;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Print_Service
{
    [RunInstaller(true)]
    public partial class Service1 : ServiceBase
    {
        int ScheduleTime = Convert.ToInt32(ConfigurationSettings.AppSettings["ThreadTime"]);
        public Thread worker = null;
        public Service1()
        {
            InitializeComponent();
        }
        public void OnDeBUG()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                ThreadStart start = new ThreadStart(Working);
                worker = new Thread(start);
                worker.Start();

            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
        public void Working()
        {
            while (true)
            {
                string path = "E:\\sample.txt";

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    Form1.PrintDocumentMethod.Printing("OneNote for Windows 10");
                    writer.WriteLine(string.Format("window services is call on " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                    
                    writer.Close();
                }

                Thread.Sleep(ScheduleTime * 60 * 1000);
            }
        }
        protected override void OnStop()
        {
            try
            {
                if ((worker != null) && worker.IsAlive)
                {
                    worker.Abort();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
