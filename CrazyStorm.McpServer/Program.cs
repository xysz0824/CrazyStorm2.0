/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.IO;
using System.Text;

namespace CrazyStorm.McpServer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                new McpProtocol(new CrazyStormTools()).Run(Console.In, Console.Out, Console.Error);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
}
