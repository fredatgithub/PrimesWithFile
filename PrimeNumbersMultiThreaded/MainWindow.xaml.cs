using System;
using System.Globalization;
using System.Windows.Threading;

namespace PrimeNumbersMultiThreaded
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _notAPrime;

        public delegate void NextPrimeDelegate();

        //Current number to check  
        private long _num = 3;

        private bool _continueCalculating;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartOrStop(object sender, EventArgs e)
        {
            if (_continueCalculating)
            {
                _continueCalculating = false;
                StartStopButton.Content = "Resume";
            }
            else
            {
                _continueCalculating = true;
                StartStopButton.Content = "Stop";
                StartStopButton.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new NextPrimeDelegate(CheckNextNumber));
            }
        }

        public void CheckNextNumber()
        {
            // Reset flag.
            _notAPrime = false;

            for (long i = 3; i <= Math.Sqrt(_num); i++)
            {
                if (_num % i == 0)
                {
                    // Set not a prime flag to true.
                    _notAPrime = true;
                    break;
                }
            }

            // If a prime number. 
            if (!_notAPrime)
            {
                BigPrime.Text = _num.ToString(CultureInfo.InvariantCulture);
            }

            _num += 2;
            if (_continueCalculating)
            {
                StartStopButton.Dispatcher.BeginInvoke(
                    DispatcherPriority.SystemIdle,
                    new NextPrimeDelegate(CheckNextNumber));
            }
        }

        private void StartOrStop(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_continueCalculating)
            {
                _continueCalculating = false;
                StartStopButton.Content = "Resume";
            }
            else
            {
                _continueCalculating = true;
                StartStopButton.Content = "Stop";
                StartStopButton.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new NextPrimeDelegate(CheckNextNumber));
            }
        }
    }
}