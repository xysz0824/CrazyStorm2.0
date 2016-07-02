using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CrazyStorm.Common
{
    public class LogHelper
    {
        static string logFilePath;
        public static void Clear(string filePath, string title)
        {
            logFilePath = filePath;
            using (StreamWriter writer = new StreamWriter(logFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine(title + " Log");
                writer.WriteLine("--------------------------------------------------------------");
                writer.WriteLine("CPU : " + EnvironmentInfoHelper.ProcessorName);
                writer.WriteLine("Graphics Card : " + EnvironmentInfoHelper.GraphicsCardName);
                writer.WriteLine("System Version : " + EnvironmentInfoHelper.OSVersion);
                writer.WriteLine(".NET Version : " + Environment.Version);
                writer.WriteLine("--------------------------------------------------------------");
            }
        }
        public static void Info(string infoMessage)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
            {
                writer.WriteLine(string.Format("{0} - [INFO]{1}", DateTime.Now, infoMessage));
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
