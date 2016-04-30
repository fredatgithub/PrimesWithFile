using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Numerics;

namespace PrimeWithFile
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    private bool _notAPrime;
    private readonly string _fileName = SettingsApplication.Default.FileName;
    private readonly string _fileCounter = SettingsApplication.Default.FileCounter;
    private List<ulong> _listNumbers = new List<ulong>();
    private List<BigInteger> _listNumbersBigInt = new List<BigInteger>();
    public delegate void NextPrimeDelegate();
    public delegate void NextPrimeDelegateBigInt();
    public Priority CurrentPriority = Priority.ForSpeed;

    public delegate void CalculateTimeSpentDelegate();

    private bool _chronoStarted;
    private DateTime _elapseTime;
    private int _numberOfDays;

    /// <summary>
    /// Current number to check, 3 by default.
    /// </summary>
    private ulong _num = 3;

    private BigInteger  _bigIntNumber = 3;

    private bool _continueCalculating;

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
      if (_chronoStarted)
      {
        _elapseTime = _elapseTime.AddSeconds(1);
        if (_elapseTime.ToString("T") == "00:00:00")
        {
          _numberOfDays++;
        }

        if (CurrentPriority == Priority.ForSpeed)
        {
          ChronoElapsed.Text = "Time spent is " + _numberOfDays + " days and " + _elapseTime.ToString("T");
        }
        else
        {
          ChronoElapsed.Text = "Time spent is " + DisplayDays(_numberOfDays) + _elapseTime.ToString("T");
        }
      }
    }

    private static string DisplayDays(int number)
    {
      return number > 0 ? number + " day" + Plural(number) + " and " : string.Empty;
    }

    private static string Plural(int number)
    {
      return number > 0 ? "s" : string.Empty;
    }

    private void StartOrStop(object sender, EventArgs e)
    {
      if (_continueCalculating)
      {
        _continueCalculating = false;
        StartStopButton.Content = "Resume";
        _chronoStarted = false;
        LabelSolution.Text = "ListNumber has " + string.Format("{0:n0}", _listNumbers.Count) + " prime numbers found.";
      }
      else
      {
        LabelSolution.Text = "ListNumber has " + string.Format("{0:n0}", _listNumbers.Count) + " prime numbers found.";
        _continueCalculating = true;
        StartStopButton.Content = "Stop";
        _chronoStarted = true;
        StartStopButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextPrimeDelegate(CheckNextNumber));
        //StartStopButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CalculateTimeSpentDelegate(CalculateTimeSpent));
      }
    }

    /// <summary>
    /// Check if next number is a prime
    /// </summary>
    public void CheckNextNumber()
    {
      // Reset flag.
      _notAPrime = false;

      for (ulong i = 3; i <= Math.Sqrt(_num); i = i + 2)
      {
        if (_num % i == 0)
        {
          _notAPrime = true;
          break;
        }
      }

      // If it is a prime number. 
      if (!_notAPrime)
      {
        string numberPrimefound = _listNumbers.Count.ToString(CultureInfo.CurrentCulture);
        BigPrime.Text = string.Format("{0:n0}", _num);
        LabelSolution.Text = string.Format("ListNumber has {0:n0} prime numbers found.", numberPrimefound);
        _listNumbers.Add(_num);
        LabelSolution.Text = string.Format("ListNumber has {0:n0} prime numbers found.", _listNumbers.Count);
      }

      if (_num >= ulong.MaxValue - 2)
      {
        _continueCalculating = false;
        MessageBox.Show(@"Max value of uLong has been reached, please close this application to save calculated numbers.");
        return;
      }

      if (Environment.WorkingSet > 50000000) //50 MB RAM or file.txt > 50 MB
      {
        // on sauve et on continue
        SaveAndEmpty();

        // ensuite on vérifie que le fichier ne dépasse 50 MB sinon on créé un nouveau fichier.
        CreateNewFile();
      }

      _num += 2;
      LabelSolution.Text = string.Format("ListNumber has {0:n0} prime numbers found.", _listNumbers.Count);
      if (_continueCalculating)
      {
        StartStopButton.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new NextPrimeDelegate(CheckNextNumber));
      }
    }

    private void CreateNewFile()
    {
      // calculating _fileName file size
      FileInfo fileinfo = new FileInfo(_fileName);
      if (fileinfo.Length <= 50000000)
      {
        return;
      }

      File.Move(_fileName, _fileName.Substring(0, _fileName.Length - 4) + _num + ".txt");
      // TODO move file to zipped_archives and start process zip it
      ZipFile();

      // On créé un nouveau fichier
      using (StreamWriter sw = new StreamWriter(_fileName))
      {
        sw.Write(string.Empty);
      }
    }

    private static void ZipFile()
    {
      //string destinationPath = "\\zipped_archives";
      //File.Move(fichier, Path.Combine(destinationPath, fichier));
      try
      {
        Process zipper = new Process
        {
          StartInfo = {UseShellExecute = true, CreateNoWindow = false, FileName = "zip_archives2.cmd"}
        };

        zipper.Start();
      }
      catch (Exception exception)
      {
        MessageBox.Show("Erreur " + exception.Message);
      }
    }

    private string ConvertToGbMbKb(FileInfo fileInfo)
    {
      string length = fileInfo.Length.ToString(CultureInfo.CurrentCulture);
      if (fileInfo.Length >= (1 << 30))
      {
        length = string.Format("{0}Gb", fileInfo.Length >> 30);
      }
      else if (fileInfo.Length >= (1 << 20))
      {
        length = string.Format("{0}Mb", fileInfo.Length >> 20);
      }
      else if (fileInfo.Length >= (1 << 10))
      {
        length = string.Format("{0}Kb", fileInfo.Length >> 10);
      }

      return length;
    }

    private void SaveAndEmpty()
    {
      if (_listNumbers.Count != 0)
      {
        ulong lastNumber = _listNumbers[_listNumbers.Count - 1];
        // T last = list[list.Count-1];
        using (StreamWriter sw = new StreamWriter(_fileName, true, Encoding.ASCII))
        {
          foreach (ulong number in _listNumbers)
          {
            sw.WriteLine(number);
          }
        }

        // TODO move file to zipped_archives and start process zip it

        // writing lastnumber in the first line of file
        using (StreamWriter sw = new StreamWriter(_fileCounter, false))
        {
          sw.WriteLine(lastNumber);
        }
      }

      _listNumbers = new List<ulong>();
    }

    private void StartOrStop(object sender, RoutedEventArgs e)
    {
      if (_continueCalculating)
      {
        _continueCalculating = false;
        StartStopButton.Content = "Resume Ulong";
        _chronoStarted = false;
      }
      else
      {
        _continueCalculating = true;
        StartStopButton.Content = "Stop Ulong";
        _chronoStarted = true;
        StartStopButton.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextPrimeDelegate(CheckNextNumber));
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _chronoStarted = false;
      ChronoElapsed.Text = "Time spent is " + _elapseTime.ToString("T");
      if (!File.Exists(_fileName))
      {
        using (StreamWriter sw = new StreamWriter(_fileName))
        {
          // List of all primes already calculated
          sw.WriteLine("2");
          sw.WriteLine("3");
          sw.WriteLine("5");
          sw.WriteLine("7");
          sw.WriteLine("11");
          sw.WriteLine("13");
          sw.WriteLine("17");
        }

        using (StreamWriter sw = new StreamWriter(_fileCounter))
        {
          sw.WriteLine("19");
        }

        _num = 21;
      }
      else
      {
        CreateFileCounter();
        string number = "3";
        using (StreamReader sr = new StreamReader(_fileCounter))
        {
          number = sr.ReadLine();
        }

        number = number.Trim();
        number = number.Replace(" ", string.Empty);
        _num = ulong.Parse(number) + 2;
      }

      ULongMaxValue.Text += " " + AddCommaInNumber(ulong.MaxValue);
      BigPrime.Text = _num.ToString(CultureInfo.CurrentCulture);
      DisplayTitle();
      GetWindowValue();
    }

    private void GetWindowValue()
    {
      Width = SettingsApplication.Default.WindowWidth < 395 ? 395 : SettingsApplication.Default.WindowWidth;
      Height = SettingsApplication.Default.WindowHeight < 180 ? 180 : SettingsApplication.Default.WindowHeight;
      Top = SettingsApplication.Default.WindowTop < 0 ? 0 : SettingsApplication.Default.WindowTop;
      Left = SettingsApplication.Default.WindowLeft < 0 ? 0 : SettingsApplication.Default.WindowLeft;

      if (SettingsApplication.Default.WindowTop + SettingsApplication.Default.WindowHeight > SystemParameters.PrimaryScreenHeight)
      {
        Top = SystemParameters.PrimaryScreenHeight - SettingsApplication.Default.WindowHeight;
      }

      if (SettingsApplication.Default.WindowLeft + SettingsApplication.Default.WindowWidth > SystemParameters.PrimaryScreenWidth)
      {
        Left = SystemParameters.PrimaryScreenWidth - SettingsApplication.Default.WindowWidth;
      }
    }

    private void DisplayTitle()
    {
      string result = string.Empty;
      try
      {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        result = string.Format("{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart,
          fvi.FilePrivatePart);
      }
      catch (Exception)
      {
        result = string.Empty;
      }
      finally
      {
        Title += result;
      }
    }

    private string AddSpaceInNumberWithDecimal(ulong number)
    {
      return String.Format("{0:n}", number);
    }

    private string AddSpaceInNumber(ulong number)
    {
      return String.Format("{0:n0}", number);
    }

    private static string AddCommaInNumber(ulong number)
    {
      return string.Format("{0:#,#}", number);
    }

    private void CreateFileCounter()
    {
      if (!File.Exists(_fileCounter))
      {
        string counter = "3";
        using (StreamReader sr = new StreamReader(_fileName))
        {
          while (!sr.EndOfStream)
          {
            counter = sr.ReadLine();
          }
        }

        using (StreamWriter sw = new StreamWriter(_fileCounter))
        {
          sw.WriteLine(counter);
        }
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      SaveAndEmpty();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // MessageBox.Show("we are closing the application so please wait for saving all calculated numbers");
      _continueCalculating = false;
      Thread.Sleep(1000);
      SaveWindowValue();
    }

    private void SaveWindowValue()
    {
      SettingsApplication.Default.WindowHeight = Height;
      SettingsApplication.Default.WindowWidth = Width;
      SettingsApplication.Default.WindowTop = Top;
      SettingsApplication.Default.WindowLeft = Left;
      SettingsApplication.Default.Save();
    }

    private void StartOrStopBigInt(object sender, RoutedEventArgs e)
    {
      if (_continueCalculating)
      {
        _continueCalculating = false;
        StartStopButtonBigInt.Content = "Resume BigInt";
        _chronoStarted = false;
      }
      else
      {
        _continueCalculating = true;
        StartStopButtonBigInt.Content = "Stop BigInt";
        _chronoStarted = true;
        StartStopButtonBigInt.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextPrimeDelegateBigInt(CheckNextNumberBigInt));
      }
    }

    public void CheckNextNumberBigInt()
    {
      // Reset flag.
      _notAPrime = false;

      for (BigInteger i = 3; i <= (BigInteger)Math.Sqrt(_num); i = i + 2) // does _bigIntNumber can be Math.squared out.
      {
        if (_num % i == 0)
        {
          _notAPrime = true;
          break;
        }
      }

      // If it is a prime number. 
      if (!_notAPrime)
      {
        string numberPrimefoundBigInt = _listNumbersBigInt.Count.ToString(CultureInfo.CurrentCulture);
        BigPrimeBigInt.Text = string.Format("{0:n0}", _bigIntNumber);
        LabelSolutionBigInt.Text = string.Format("ListNumber has {0} prime numbers found.", _bigIntNumber);
        _listNumbersBigInt.Add(_bigIntNumber);
        LabelSolutionBigInt.Text = string.Format("ListNumber has {0:n0} prime numbers found.", _listNumbersBigInt.Count);
      }

      if (Environment.WorkingSet > 50000000) //50 MB RAM or file.txt > 50 MB
      {
        // on sauve et on continue
        SaveAndEmptyBigInt();

        // ensuite on vérifie que le fichier ne dépasse 50 MB sinon on créé un nouveau fichier.
        CreateNewFileBigInt();
      }

      _num += 2;
      LabelSolutionBigInt.Text = string.Format("ListNumber has {0:n0} prime numbers found.", _listNumbersBigInt.Count);
      if (_continueCalculating)
      {
        StartStopButtonBigInt.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new NextPrimeDelegateBigInt(CheckNextNumberBigInt));
      }
    }

    private void SaveAndEmptyBigInt()
    {
      if (_listNumbersBigInt.Count != 0)
      {
        BigInteger lastNumber = _listNumbersBigInt[_listNumbersBigInt.Count - 1];
        // T last = list[list.Count-1];
        using (StreamWriter sw = new StreamWriter(_fileName, true, Encoding.ASCII))
        {
          foreach (BigInteger number in _listNumbersBigInt)
          {
            sw.WriteLine(number);
          }
        }

        // writing lastnumber in the first line of file
        using (StreamWriter sw = new StreamWriter(_fileCounter, false))
        {
          sw.WriteLine(lastNumber);
        }
      }

      _listNumbersBigInt = new List<BigInteger>();
    }

    private void CreateNewFileBigInt()
    {
      // calculating _fileName file size
      FileInfo fileinfo = new FileInfo(_fileName);
      if (fileinfo.Length <= 50000000)
      {
        return;
      }

      File.Move(_fileName, _fileName.Substring(0, _fileName.Length - 4) + _num + ".txt");
      // TODO move file to zipped_archives and start process zip it
      ZipFile();

      // On créé un nouveau fichier
      using (StreamWriter sw = new StreamWriter(_fileName))
      {
        sw.Write(string.Empty);
      }
    }

    private void StartOrStopIntegers(object sender, RoutedEventArgs e)
    {
      //integers-class method

    }
  }
}