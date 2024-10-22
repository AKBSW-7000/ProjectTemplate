
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Projekt1.UIComponents;

public partial class DeviceStatusNButton : Button, INotifyPropertyChanged
{
    public IObservableDevice device;
    public string            Text
    {
        get;
    }
    public string imageURI
    {
        get;
    }
    public bool ConnectivityStatus
    {
        get;
        private set;
    }
    public SolidColorBrush FillColor => ConnectivityStatus ? Brushes.Chartreuse:Brushes.Red;

    private Timer connectionPoll;
    public DeviceStatusNButton(IObservableDevice idi)
    {
        InitializeComponent();
        device = idi;
        Text   = device.GetType().Name;
        ConnectivityStatus = device.getConnectedStatus();

        imageURI = device.getIconURI();
        if (string.IsNullOrEmpty(imageURI)) imageView.Visibility = Visibility.Collapsed;

        RaisePropertyChanged();
        connectionPoll   =  new Timer(3000){AutoReset = true};
        connectionPoll.Elapsed += ConnectionPoll_Elapsed;
        IsVisibleChanged += DeviceStatusNButton_IsVisibleChanged;
    }

    private void ConnectionPoll_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (connectionPoll != null)
        {
            connectionPoll.Stop();
            ConnectivityStatus = device.getConnectedStatus();
            RaisePropertyChanged();
            connectionPoll.Start();
        }
    }

    private void DeviceStatusNButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
            connectionPoll?.Start();
        else
            connectionPoll?.Stop();
    }

    #region PublicEvent

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged()
    {
        foreach(var property in GetType().GetProperties())
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property.Name));
        
    }

#endregion //PublicEvent
}

