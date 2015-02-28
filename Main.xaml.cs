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
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
        }

        private void EmitterImage_MouseEnter(object sender, MouseEventArgs e)
        {
            EmitterImage.Source = new BitmapImage(new Uri(@"Images\button1on.png", UriKind.Relative));
        }

        private void EmitterImage_MouseLeave(object sender, MouseEventArgs e)
        {
            EmitterImage.Source = new BitmapImage(new Uri(@"Images\button1.png", UriKind.Relative));
        }

        private void LaserImage_MouseEnter(object sender, MouseEventArgs e)
        {
            LaserImage.Source = new BitmapImage(new Uri(@"Images\button2on.png", UriKind.Relative));
        }

        private void LaserImage_MouseLeave(object sender, MouseEventArgs e)
        {
            LaserImage.Source = new BitmapImage(new Uri(@"Images\button2.png", UriKind.Relative));
        }

        private void MaskImage_MouseEnter(object sender, MouseEventArgs e)
        {
            MaskImage.Source = new BitmapImage(new Uri(@"Images\button3on.png", UriKind.Relative));
        }

        private void MaskImage_MouseLeave(object sender, MouseEventArgs e)
        {
            MaskImage.Source = new BitmapImage(new Uri(@"Images\button3.png", UriKind.Relative));
        }

        private void ReboundImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ReboundImage.Source = new BitmapImage(new Uri(@"Images\button4on.png", UriKind.Relative));
        }

        private void ReboundImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ReboundImage.Source = new BitmapImage(new Uri(@"Images\button4.png", UriKind.Relative));
        }

        private void ForceImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ForceImage.Source = new BitmapImage(new Uri(@"Images\button5on.png", UriKind.Relative));
        }

        private void ForceImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ForceImage.Source = new BitmapImage(new Uri(@"Images\button5.png", UriKind.Relative));
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimeScale.Width = ActualWidth - 200;
        }
    }
}
