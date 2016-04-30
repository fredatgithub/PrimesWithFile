using System;
using System.Windows;
using System.Windows.Threading;

namespace WpfTimerDemo
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            // Register method target to execute every tick
            dispatcherTimer.Tick += DispatcherTimerTick;

            // One tick every second
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            // Start the timer
            dispatcherTimer.Start();
        }

         

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            // Code to execute every tick
            LabelTimer1.Content = "Time : " + DateTime.Now.ToString("T"); // "T" = long time
        }

        private void StartStop_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}