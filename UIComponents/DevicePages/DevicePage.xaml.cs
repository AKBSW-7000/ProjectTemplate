using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

using Newtonsoft.Json;

namespace Projekt1;
public partial class DevicePage : UserControl
{
    System.Timers.Timer _timer;
    bool interrupt = false;
    public DevicePage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
        _timer = new System.Timers.Timer()
        {
            Interval = 100,
            AutoReset = false,
            Enabled = false,
        };
        _timer.Elapsed += _timer_Elapsed;
        IsVisibleChanged += DevicePage_IsVisibleChanged;
    }

    private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _timer.Stop();
        try
        {
            var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
            App.DataModel.Fstatus = festo.ControllerStatus;
            App.DataModel.FdiagMsg = festo.DiagnosticMsg;
            App.DataModel.Fposition = festo.CurrentPostion;
            App.DataModel.Fvelocity = festo.CurrentVelocity;
        }
        finally { }
        _timer.Start();
    
    }
    private void DevicePage_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
    }

    private void StepMoveUp_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        controls.IsEnabled = false;
        EStop.Visibility = Visibility.Visible;
        Task.Run(() =>
        {
            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt) ? flt : throw new InvalidCastException();
                float? vel = null;
                if (float.TryParse(App.DataModel.VelocityValue, out float velf) && velf>0)
                    vel = velf;
                festo.move_rel(ref interrupt, fltPos, vel, null);
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    controls.IsEnabled = true;
                    interrupt = false;
                    EStop.Visibility = Visibility.Collapsed;
                });
            }
        });
    }

    private void StepMoveDown_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        controls.IsEnabled = false;
        EStop.Visibility = Visibility.Visible;
        Task.Run(() =>
        {
            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt) ? flt : throw new InvalidCastException();
                float? vel = null;
                if (float.TryParse(App.DataModel.VelocityValue, out float velf) && velf > 0)
                    vel = velf;
                festo.move_rel(ref interrupt, fltPos, vel, null);
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    controls.IsEnabled = true;
                    interrupt = false;
                    EStop.Visibility = Visibility.Collapsed;
                });
            }
        });
    }

    private void GoTo_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        controls.IsEnabled = false;
        EStop.Visibility = Visibility.Visible;
        Task.Run(() =>
        {
            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt) ? flt : throw new InvalidCastException();
                float? vel = null;
                if(float.TryParse(App.DataModel.VelocityValue, out float velf) && velf > 0)
                    vel= velf;
                festo.move_abs(ref interrupt, fltPos, vel, null);
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    controls.IsEnabled = true;
                    interrupt = false; 
                    EStop.Visibility = Visibility.Collapsed;
                });
            }
        });
    }

    private void stop_immediate_click(object sender, System.Windows.RoutedEventArgs e)
    {
        interrupt = true;
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        controls.IsEnabled = false;
        Task.Run(() =>
        {
            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                festo.home();
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    controls.IsEnabled = true;
                });
            }
        });

    }

    private void Ack_Festo_Click(object sender, RoutedEventArgs e)
    {
        controls.IsEnabled = false;
        Task.Run(() =>
        {
            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                festo.StopFesto();
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    controls.IsEnabled = true;
                });
            }
        });
    }
}

public partial class AppDataModel : INotifyPropertyChanged
{
    private string offsetValue = "0.0"; 
    private string velocityValue = "0.0";
    [JsonIgnore]
    public string OffsetValue
    {
        get=> offsetValue; 
        set{
            if (value.EndsWith(".")||string.IsNullOrEmpty(value)) {
                offsetValue = value; 
                RaisePropertyChanged(nameof(OffsetValue));
                return;
            }

            if (!double.TryParse(value, out _)) return;
            offsetValue = value;
            RaisePropertyChanged(nameof(OffsetValue));
        }
    }
    [JsonIgnore]
    public string VelocityValue
    {
        get => velocityValue;
        set
        {
            if (value.EndsWith(".") || string.IsNullOrEmpty(value))
            {
                velocityValue = value;
                RaisePropertyChanged(nameof(VelocityValue));
                return;
            }

            if (!double.TryParse(value, out _)) return;
            velocityValue = value;
            RaisePropertyChanged(nameof(VelocityValue));
        }
    }
    private string fStatus = "-999";
    private string fDiagMsg = "-999";
    private string fPosition = "-999";
    private string fVelocity = "-999";
    private bool isStep = false;
    private bool isAbs = true;
    [JsonIgnore]
    public bool IsStep
    {
        get => isStep;
        set
        {
            isStep = value;
            if(isStep && IsAbs) IsAbs = false;
            RaisePropertyChanged(nameof(IsStep));
        }
    }
    [JsonIgnore]
    public bool IsAbs
    {
        get => isAbs;
        set
        {
            isAbs = value; 
            if (isAbs && IsStep) IsStep = false;
            RaisePropertyChanged(nameof(IsAbs));
        }
    }
    [JsonIgnore]
    public string Fstatus
    {
        get => fStatus;
        set
        {
            fStatus = value;
            RaisePropertyChanged(nameof(Fstatus));
        }
    }
    [JsonIgnore]
    public string FdiagMsg
    {
        get => fDiagMsg;
        set
        {
            fDiagMsg = value;
            RaisePropertyChanged(nameof(FdiagMsg));
        }
    }
    [JsonIgnore]
    public string Fposition
    {
        get => fPosition;
        set
        {
            fPosition = value;
            RaisePropertyChanged(nameof(Fposition));
        }
    }
    [JsonIgnore]
    public string Fvelocity
    {
        get => fVelocity;
        set
        {
            fVelocity = value;
            RaisePropertyChanged(nameof(Fvelocity));
        }
    }
}