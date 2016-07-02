using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Management;

namespace CrazyStorm.Common
{
    public class EnvironmentInfoHelper
    {
        public static string ProcessorName
        {
            get
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj2 in searcher.Get())
                {
                    try
                    {
                        return obj2.GetPropertyValue("Name").ToString();
                    }
                    catch
                    {
                        continue;
                    }
                }
                return string.Empty;
            }
        }
        public static string OSVersion
        {
            get
            {
                return Environment.OSVersion + "(" + OSBit + "-bit)";
            }
        }
        public static string OSBit
        {
            get
            {
                ConnectionOptions oConn = new ConnectionOptions();
                System.Management.ManagementScope oMs = new System.Management.ManagementScope("\\\\localhost", oConn);
                System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("select AddressWidth from Win32_Processor");
                ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
                ManagementObjectCollection oReturnCollection = oSearcher.Get();
                string addressWidth = null;

                foreach (ManagementObject oReturn in oReturnCollection)
                {
                    addressWidth = oReturn["AddressWidth"].ToString();
                }

                return addressWidth;
            }
        }
        public static string GraphicsCardName
        {
            get
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_VideoController");
                foreach (ManagementObject mo in mos.Get())
                {
                    try
                    {
                        return mo["Name"].ToString();
                    }
                    catch
                    {
                        continue;
                    }
                }
                return string.Empty;
            }
        }
    }
}
