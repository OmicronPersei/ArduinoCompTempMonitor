using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace ArduinoCompTempMonitor
{
    class OpenHardwareMon
    {
        //private List<string> _identifiers;
        private List<OpenHardwareMon.SensorValueKey> _theValues;
        private List<string> _identifiers;
        private ManagementObjectSearcher _searcher;
        
        public OpenHardwareMon(List<string> identifiers)
        {
            _theValues = new List<SensorValueKey>();
            _identifiers = identifiers;

            _searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
        }

        public void getValues()
        {
            
            string identifier;
            _theValues.Clear();

            foreach (ManagementObject obj in _searcher.Get())
            {
                identifier = obj["Identifier"].ToString();

                foreach (string iden in _identifiers)
                {
                    if (identifier == iden)
                    {
                        //found a match, create a key
                        SensorValueKey aKey = new SensorValueKey();
                        aKey._identifier = identifier;
                        aKey._mo = obj;

                        _theValues.Add(aKey);

                        break;
                    }
                }
            }


        }

        public string getVal(string identifier, string property)
        {
            string buff = "";

            foreach (OpenHardwareMon.SensorValueKey theKey in _theValues)
            {
                if (theKey._identifier == identifier)
                {
                    //found match
                    try
                    {
                        buff = theKey._mo[property].ToString();
                    }
                    catch
                    {
                        //nothing
                    }
                    
                    break;
                }
            }

            return buff;
        }


        public void ProbeWPI()
        {
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor");
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject mo in osDetailsCollection)
            {
                foreach (PropertyData prop in mo.Properties)
                {
                    Console.WriteLine("{0}: {1}", prop.Name, prop.Value);
                    //Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());

                }

                Console.WriteLine("\n");
            }
        }

        public class SensorValueKey
        {
            public string _identifier;
            public ManagementObject _mo;
            
            public SensorValueKey()
            {
                //nothing yet
            }
        }


    }
}
