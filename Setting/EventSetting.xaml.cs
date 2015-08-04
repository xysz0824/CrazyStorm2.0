/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrazyStorm
{
    /// <summary>
    /// Interaction logic for EventSetting.xaml
    /// </summary>
    public partial class EventSetting : Window
    {
        public EventSetting()
        {
            InitializeComponent();
            var color = Color.FromRgb(96,182,236);
            DrawHelper.DrawEllipse(CurveEditor, 0, 144, 5, color, 1);
            DrawHelper.DrawEllipse(CurveEditor, 360, 0, 5, color, 1);
            DrawHelper.DrawLine(CurveEditor, 0, 144, 360, 0, 3, color, 1);
        }
    }
}
