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
            member.Add(new Property() { Name = "起始帧", Value = "" });
            member.Add(new Property() { Name = "持续帧", Value = "" });
            member.Add(new Property() { Name = "坐标", Value = "" });
            member.Add(new Property() { Name = "速度", Value = "" });
            member.Add(new Property() { Name = "速度角", Value = "" });
            member.Add(new Property() { Name = "加速度", Value = "" });
            member.Add(new Property() { Name = "加速度角", Value = "" });
            ComponentGrid.DataContext = member;
            ObservableCollection<Property> member2 = new ObservableCollection<Property>();
            member2.Add(new Property() { Name = "坐标", Value = "" });
            member2.Add(new Property() { Name = "条数", Value = "" });
            member2.Add(new Property() { Name = "周期", Value = "" });
            member2.Add(new Property() { Name = "发射角", Value = "" });
            member2.Add(new Property() { Name = "范围角", Value = "" });
            EmitterGrid.DataContext = member2;
            ObservableCollection<Property> member3 = new ObservableCollection<Property>();
            member3.Add(new Property() { Name = "生命", Value = "" });
            member3.Add(new Property() { Name = "类型", Value = "" });
            member3.Add(new Property() { Name = "宽度比", Value = "" });
            member3.Add(new Property() { Name = "高度比", Value = "" });
            member3.Add(new Property() { Name = "RGB", Value = "" });
            member3.Add(new Property() { Name = "不透明度", Value = "" });
            member3.Add(new Property() { Name = "旋转", Value = "" });
            member3.Add(new Property() { Name = "旋转速度方向相关", Value = "" });
            member3.Add(new Property() { Name = "旋转保持比例", Value = "" });
            member3.Add(new Property() { Name = "速度", Value = "" });
            member3.Add(new Property() { Name = "速度角", Value = "" });
            member3.Add(new Property() { Name = "加速度", Value = "" });
            member3.Add(new Property() { Name = "加速度角", Value = "" });
            member3.Add(new Property() { Name = "雾化效果", Value = "" });
            member3.Add(new Property() { Name = "消隐效果", Value = "" });
            member3.Add(new Property() { Name = "拖影效果", Value = "" });
            member3.Add(new Property() { Name = "混合模式", Value = "" });
            member3.Add(new Property() { Name = "出屏即消", Value = "" });
            member3.Add(new Property() { Name = "开启碰撞", Value = "" });
            member3.Add(new Property() { Name = "不受遮罩影响", Value = "" });
            member3.Add(new Property() { Name = "不受反弹板影响", Value = "" });
            member3.Add(new Property() { Name = "不受力场影响", Value = "" });
            ParticleGrid.DataContext = member3;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            double[] width = new double[] { ComponentKey.ActualWidth, EmitterKey.ActualWidth, ParticleKey.ActualWidth };
            double maxWidth = width.Max();
            ComponentKey.Width = maxWidth;
            EmitterKey.Width = maxWidth;
            ParticleKey.Width = maxWidth;
        }
    }
}
