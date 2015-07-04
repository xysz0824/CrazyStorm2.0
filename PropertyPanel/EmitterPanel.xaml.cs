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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrazyStorm.Core;
using System.Collections.ObjectModel;

namespace CrazyStorm
{
    /// <summary>
    /// PropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class EmitterPanel : UserControl
    {
        public EmitterPanel(Emitter emitter)
        {
            InitializeComponent();
            ObservableCollection<Property> member = new ObservableCollection<Property>();
            member.Add(new Property() { Name = "初始帧", Value = "" });
            member.Add(new Property() { Name = "持续帧", Value = "" });
            member.Add(new Property() { Name = "X坐标", Value = "" });
            member.Add(new Property() { Name = "Y坐标", Value = "" });
            member.Add(new Property() { Name = "速度", Value = "" });
            member.Add(new Property() { Name = "速度角", Value = "" });
            member.Add(new Property() { Name = "加速度", Value = "" });
            member.Add(new Property() { Name = "加速度角", Value = "" });
            ComponentGrid.DataContext = member;
            ObservableCollection<Property> member2 = new ObservableCollection<Property>();
            member2.Add(new Property() { Name = "X坐标", Value = "" });
            member2.Add(new Property() { Name = "Y坐标", Value = "" });
            member2.Add(new Property() { Name = "条数", Value = "" });
            member2.Add(new Property() { Name = "周期", Value = "" });
            member2.Add(new Property() { Name = "发射角", Value = "" });
            member2.Add(new Property() { Name = "范围角", Value = "" });
            EmitterGrid.DataContext = member2;
        }
    }
}
