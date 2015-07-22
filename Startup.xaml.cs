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
using System.Windows.Threading;

namespace CrazyStorm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members
        //Control the startup
        Main mainWindow;
        DispatcherTimer dTimer = new DispatcherTimer();
        short frame = 0;
        #endregion

        #region Constructor
        public MainWindow()
        {
            Script.Lexer lexer = new Script.Lexer();
            lexer.Load("_12345(0, 10.231123213123, rand(-5, -2), -((x + y) / saturate(z)))");
            Script.Parser parser = new Script.Parser(lexer);
            Script.SyntaxTree tree = parser.Expression();

            InitializeComponent();

            dTimer.Tick += dTimer_Tick;
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dTimer.Start();

            mainWindow = new Main();
        }
        #endregion

        #region Private Methods
        private void dTimer_Tick(object sender, EventArgs e)
        {
            //a frame approximately equal to 16ms(because 60 frames equals one second)
            frame++;
            if (Opacity < 1.0f) 
                Opacity += 0.1f;

            if (frame >= 60)
            {
                dTimer.Stop();
                //Enter main ui
                mainWindow.Show();
                this.Close();
            }
        }
        #endregion
    }
}
