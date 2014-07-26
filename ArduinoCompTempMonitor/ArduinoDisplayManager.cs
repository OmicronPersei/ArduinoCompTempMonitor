using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace ArduinoCompTempMonitor
{
    class ArduinoDisplayManager
    {
        ArduinoScreenCommunicator _theScreenCom;
        OpenHardwareMon _openHardwareMon;
        System.Timers.Timer _timer;

        private Boolean _enabled;

        private int _screenLength = 20;

        //These variables represent the identifiers that OpenHardwareMonitor uses to identify different sensor values.
        //We need to list these such that the OpenHardwareMon class gets these values and stores them when calling OpenHardwareMon.getValues()
        //Once this is called, the values are then available to be retrieved. using OpenHardwareMon.getVal(identifier, property)
        const string _CPUtemp = "/amdcpu/0/temperature/0";
        const string _RAMused = "/ram/data/0";
        const string _RAMunused = "/ram/data/1";
        const string _GPUtemp = "/nvidiagpu/0/temperature/0";
        const string _GPUfanspeed = "/nvidiagpu/0/control/0";
        const string _CPUfan = "/lpc/it8728f/fan/0";

        public ArduinoDisplayManager()
        {
            List<string> identifiers = new List<string>();
            identifiers.Add(_CPUtemp);
            identifiers.Add(_RAMused);
            identifiers.Add(_RAMunused);
            identifiers.Add(_GPUtemp);
            identifiers.Add(_GPUfanspeed);
            identifiers.Add(_CPUfan);
            
            _openHardwareMon = new OpenHardwareMon(identifiers);

            _enabled = false;
            
        }

        public void start(int updateInterval, string COMport)
        {
            if (!_enabled)
            {
                _theScreenCom = new ArduinoScreenCommunicator(COMport, _screenLength);
                
                _timer = new System.Timers.Timer(updateInterval);
                _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                _timer.Enabled = true;

                _enabled = true;

                //attempt to send the starting string
                List<string> lines = new List<string>();
                lines.Add("Starting monitoring.");
                _theScreenCom.SendLines(lines);

            }
        }

        public void stop()
        {
            if (_enabled)
            {
                _timer.Enabled = false;

                //attempt to send monitor stopped string
                List<string> lines = new List<string>();
                lines.Add("Monitoring stopped.");
                _theScreenCom.SendLines(lines);

                _theScreenCom.Close();

                _enabled = false;
            }
        }

        /*
         * public TimeSpan UpTime {
            get {
                using (var uptime = new PerformanceCounter("System", "System Up Time")) {
                    uptime.NextValue();       //Call this an extra time before reading its value
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
        }
         * 
         * */


        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            displayValues();
        }

        private string getCPUUptime()
        {
            TimeSpan theUptime = GetUptime();

            if (theUptime.Ticks != 0)
            {
                //Up: 00d 00h 00m 00s
                return "Up: " + theUptime.Days.ToString("00") + "d " + theUptime.Hours.ToString("00") + "h " + theUptime.Minutes.ToString("00") + "m " + theUptime.Seconds.ToString("00") + "s";
            }
            else
            {
                return "unable to get uptime";
            }
        }

        private static TimeSpan GetUptime()
        {
            try
            {
                ManagementObject mo = new ManagementObject(@"\\.\root\cimv2:Win32_OperatingSystem=@");
                DateTime lastBootUp = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
                return DateTime.Now.ToUniversalTime() - lastBootUp.ToUniversalTime();
            }
            catch
            {
                return new TimeSpan(0);
            }
            
        }

        private void displayValues()
        {
            //(dashes are width of the screen)
            //--------------------
            //CPU temp: 30/25/55 C
            //RAM used: 2048MB 25%
            //GPU temp: 60 C

            List<string> lines = new List<string>();
            
            try
            {
                _openHardwareMon.getValues();

                //calculate RAM percentage used
                //First, get total ram
                double usedRAM = Convert.ToDouble(_openHardwareMon.getVal(_RAMused, "Value"));
                double totalRAM = Convert.ToDouble(_openHardwareMon.getVal(_RAMunused, "Value")) + usedRAM;
                string RAMvalue = Math.Round(usedRAM, 2).ToString("0.00");
                double percentage = Math.Round(((double)(usedRAM / totalRAM)) * 100);

                string CPUtempvalue = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_CPUtemp, "Value"))).ToString();
                string CPUtempmin = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_CPUtemp, "Min"))).ToString();
                string CPUtempmax = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_CPUtemp, "Max"))).ToString();

                string CPUfanvalue = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_CPUfan, "Value"))).ToString("0000");
                string CPUfanmax = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_CPUfan, "Max"))).ToString("0000");
                
                //CPU fan: 1234/1245

                string GPUtempvalue = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUtemp, "Value"))).ToString();
                string GPUtempmin = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUtemp, "Min"))).ToString();
                string GPUtempmax = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUtemp, "Max"))).ToString();

                string GPUfanvalue = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUfanspeed, "Value"))).ToString();
                string GPUfanmin = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUfanspeed, "Min"))).ToString();
                string GPUfanmax = Math.Round(Convert.ToDouble(_openHardwareMon.getVal(_GPUfanspeed, "Max"))).ToString();

                lines.Add("CPU temp: " + CPUtempvalue + "/" + CPUtempmin + "/" + CPUtempmax + " C");
                lines.Add("CPU fan: " + CPUfanvalue + "/" + CPUfanmax);
                lines.Add("GPU temp: " + GPUtempvalue + "/" + GPUtempmin + "/" + GPUtempmax + " C");
                lines.Add("GPU fan:  " + GPUfanvalue + "/" + GPUfanmin + "/" + GPUfanmax + " %");
                lines.Add("RAM used: " + RAMvalue + "GB/" + percentage.ToString() + "%");
                lines.Add(getCPUUptime());
                //lines.Add("fdasfasd");
                //lines.Add("dfa8wowesfjd");
            }
            catch
            {
                //Error, for some reason.
                lines.Add("ERROR:");
                lines.Add("Unable to get values.");
            }

            _theScreenCom.SendLines(lines);
        }

        public void probeWPI()
        {
            _openHardwareMon.ProbeWPI();
        }
    }
}
