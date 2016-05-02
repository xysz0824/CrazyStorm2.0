using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class LogHelper
    {
        static readonly string logFilePath = "Log.txt";
        public static void Clear()
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine(VersionInfo.AppTitle + " Log");
                writer.WriteLine("--------------------------------------------------------------");
                writer.WriteLine("CPU : " + EnviromentInfoHelper.ProcessorName);
                writer.WriteLine("Graphics Card : " + EnviromentInfoHelper.GraphicsCardName);
                writer.WriteLine("System Version : " + EnviromentInfoHelper.OSVersion);
                writer.WriteLine(".NET Version : " + Environment.Version);
                writer.WriteLine("--------------------------------------------------------------");
            }
        }
        public static void Error(string errorMessage)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
            {
                writer.WriteLine(string.Format("{0} - [ERROR]{1}", DateTime.Now, errorMessage));
            }
        }
    }
}
