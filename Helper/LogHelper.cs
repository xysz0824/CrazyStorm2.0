using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm
{
    class LogHelper
    {
        static readonly string logFilePath = "log.txt";
        public static void Clear()
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, false, Encoding.UTF8))
            {
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
