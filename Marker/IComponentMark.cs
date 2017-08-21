/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    interface IComponentMark
    {
        void Draw(Canvas canvas, Component component, int x, int y);
    }
}
