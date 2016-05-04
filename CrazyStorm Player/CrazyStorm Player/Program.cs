/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm_Player
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Player player = new Player())
            {
                player.Run();
            }
        }
    }
}
