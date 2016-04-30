using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace CrazyStorm
{
    class EnviromentInfoHelper
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
                string bit = Environment.Is64BitOperatingSystem ? "64" : "32";
                return Environment.OSVersion + "(" + bit + "-bit)";
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
