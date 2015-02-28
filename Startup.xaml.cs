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
using System.Windows.Threading;

namespace CrazyStorm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //Control the startup
        private DispatcherTimer dTimer = new DispatcherTimer();
        private short totalsecond = 1;
        private short frame = 0;
        public MainWindow()
        {
            InitializeComponent();

            dTimer.Tick += dTimer_Tick;
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dTimer.Start();
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {
            //a frame approximately equal to 16ms(because 60 frames equals one second)
            frame++;
            if (Opacity < 1.0f) 
                Opacity += 0.1f;
            if (frame >= totalsecond * 60)
            {
                dTimer.Stop();
                //Enter main ui
                Main window = new Main();
                window.Show();
                this.Close();
            }
        }
    }
}
