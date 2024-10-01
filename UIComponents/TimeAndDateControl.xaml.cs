using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Projekt1;

public partial class TimeAndDateControl : UserControl, INotifyPropertyChanged
{
    public MainWindow ownerMW
    {
        get
        {
            return (MainWindow)GetValue(ownerMWProperty);
        }
        set
        {
            SetValue(ownerMWProperty, value);
        }
    }
    // Using a DependencyProperty as the backing store for ownerMain.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ownerMWProperty =
        DependencyProperty.Register("ownerMW", typeof(MainWindow), typeof(TimeAndDateControl), new PropertyMetadata(null));

    public DateTime systemDateTime => DateTime.Now;
    private DispatcherTimer datetimeTimer;

    public TimeAndDateControl()
    {
        InitializeComponent();

        datetimeTimer = new DispatcherTimer(new TimeSpan(0, 0, 1)
                                          , DispatcherPriority.Normal
                                          , (sender, args) =>
                                          {
                                              RaisePropertyChanged("systemDateTime");
                                          }
                                          , Dispatcher.CurrentDispatcher);

        datetimeTimer.Start();
        IsVisibleChanged += TimeAndDateControl_IsVisibleChanged;
    }

    private void TimeAndDateControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        ownerMW.Closing += ((sendee, args) => datetimeTimer.Stop());

    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string property)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
