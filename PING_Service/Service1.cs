using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PING_Service
{
    public partial class Service1 : ServiceBase
    {
        Timer timer;
        DateTime LastChecked;
        public Service1()
        {
            timer = new Timer();
            //When autoreset is True there are reentrancy problme 
            timer.AutoReset = false;

            timer.Elapsed += new ElapsedEventHandler(DoStuff);
        }
        private void DoStuff(object sender, ElapsedEventArgs e)
        {

            LastChecked = DateTime.Now;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string path = "\\PING\\";
            string fullpath = Path.GetFullPath(path);
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = fullpath + "PING.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = fullpath + " 10000";
            Process process = Process.Start(startInfo);
            TimeSpan ts = DateTime.Now.Subtract(LastChecked);
            TimeSpan MaxWaitTime = TimeSpan.FromMinutes(1);


            if (MaxWaitTime.Subtract(ts).CompareTo(TimeSpan.Zero) > -1)
                timer.Interval = MaxWaitTime.Subtract(ts).Milliseconds;
            else
                timer.Interval = 1000;

            timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            timer.Interval = 1000;
            timer.Start();
        }

        protected override void OnPause()
        {

            base.OnPause();
            this.timer.Stop();

        }

        protected override void OnContinue()
        {
            base.OnContinue();
            this.timer.Interval = 1000;
            this.timer.Start();

        }

        protected override void OnStop()
        {
            base.OnStop();
            this.timer.Stop();
        }
    }
}
