using CrazyStorm.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// JumpToFrame.xaml 的交互逻辑
    /// </summary>
    public partial class JumpToFrame : Window
    {
        int maxFrame;

        public int TargetFrame { get;private set; }
        public bool Confirmed { get;private set; }
        public JumpToFrame(int currentFrame, int maxFrame)
        {
            TargetFrame = currentFrame;
            this.maxFrame = maxFrame;
            InitializeComponent();
            FrameNumber.Content = string.Format((string)FindResource("FrameNumberStr"), maxFrame);
            FrameInputBox.Text = currentFrame.ToString();
            FrameInputBox.Focus();
            FrameInputBox.CaretIndex = FrameInputBox.Text.Length;
        }
        private void FrameInputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var c in e.Text) if (!char.IsDigit(c)) e.Handled = true;
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            TargetFrame = MathUtil.Clamp(int.Parse(FrameInputBox.Text), 1, maxFrame);
            Confirmed = true;
            Close();
        }
        private void FrameInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Confirm_Click(null, null);
        }
    }
}
