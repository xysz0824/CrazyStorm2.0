/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace CrazyStorm
{
    class DoubleClickDetector
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        extern static uint GetDoubleClickTime();
        DispatcherTimer timer;
        int count;

        public void Start()
        {
            if (count == 0)
            {
                if (timer != null)
                    timer.IsEnabled = false;

                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, (int)GetDoubleClickTime());
                timer.Tick += (s, e1) =>
                    {
                        timer.IsEnabled = false;
                        count = 0;
                    };
                timer.IsEnabled = true;
            }
            count++;
        }

        public bool IsDetected()
        {
            if (count == 2)
            {
                count = 0;
                timer.IsEnabled = false;
                return true;
            }
            return false;
        }
    }
}
