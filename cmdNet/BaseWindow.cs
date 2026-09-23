using System;
using System.Windows;

namespace cmdNet
{
    public class BaseWindow : Window
    {
        private const double BaseFontSize = 16;
        private const double ReferenceWidth = 1000;

        public BaseWindow()
        {
            FontSize = BaseFontSize;
            SizeChanged += BaseWindow_SizeChanged;
        }

        private void BaseWindow_SizeChanged(
            object sender, SizeChangedEventArgs e)
        {
            double scale = ActualWidth / ReferenceWidth;
            scale = Math.Clamp(scale, 1.0, 1.3);

            FontSize = BaseFontSize * scale;
        }
    }
}