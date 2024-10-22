using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using AKBUtilities;
using Newtonsoft.Json;

namespace Projekt1.UIComponents
{
    public partial class FestoDevice : UserControl
    {
        System.Timers.Timer _timer;
        bool                interrupt = false;

        public FestoDevice()
        {
            DataContext = App.DataModel;
            InitializeComponent();

            _timer = new System.Timers.Timer()
                     {
                         Interval = 100, AutoReset = false, Enabled = false,
                     };

            _timer.Elapsed   += _timer_Elapsed;
            IsVisibleChanged += devicePage_isVisibleChanged;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();

            try
            {
                var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                App.DataModel.Fstatus   = festo.ControllerStatus;
                App.DataModel.FdiagMsg  = festo.DiagnosticMsg;
                App.DataModel.Fposition = festo.CurrentPostion;
                App.DataModel.Fvelocity = festo.CurrentVelocity;
            } finally {}

            _timer.Start();

        }

        private void devicePage_isVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
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

        private void stepMoveUp_click(object sender, System.Windows.RoutedEventArgs e)
        {
            #if !OFFLINE_DEBUG
            controls.IsEnabled = false;
            EStop.Visibility   = Visibility.Visible;

            Task.Run(() =>
                     {
                         try
                         {
                             var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;

                             var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt)
                                              ? flt
                                              : throw new InvalidCastException();

                             float? vel                                                                       = null;
                             if (float.TryParse(App.DataModel.VelocityValue, out float velf) && velf > 0) vel = velf;

                             if (!festo.move_rel(ref interrupt, fltPos, vel, null))
                                 App.errorManager.set_error((int)DefaultError.MtnMoveFail
                                                          , "Festo_StepMoveUp_Click"
                                                          , (int)DefaultErrorAction.MotionController);
                         } finally
                         {
                             Dispatcher.Invoke(() =>
                                               {
                                                   controls.IsEnabled = true;
                                                   interrupt          = false;
                                                   EStop.Visibility   = Visibility.Collapsed;
                                               });
                         }
                     });
            #endif
        }

        private void stepMoveDown_click(object sender, System.Windows.RoutedEventArgs e)
        {
            #if !OFFLINE_DEBUG
            controls.IsEnabled = false;
            EStop.Visibility   = Visibility.Visible;

            Task.Run(() =>
                     {
                         try
                         {
                             var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;

                             var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt)
                                              ? flt
                                              : throw new InvalidCastException();

                             float? vel                                                                       = null;
                             if (float.TryParse(App.DataModel.VelocityValue, out float velf) && velf > 0) vel = velf;

                             if (!festo.move_rel(ref interrupt, fltPos, vel, null))
                                 App.errorManager.set_error((int)DefaultError.MtnMoveFail
                                                          , "Festo_StepMoveDown_Click"
                                                          , (int)DefaultErrorAction.MotionController);
                         } finally
                         {
                             Dispatcher.Invoke(() =>
                                               {
                                                   controls.IsEnabled = true;
                                                   interrupt          = false;
                                                   EStop.Visibility   = Visibility.Collapsed;
                                               });
                         }
                     });
            #endif
        }

        private void goTo_click(object sender, System.Windows.RoutedEventArgs e)
        {
            #if !OFFLINE_DEBUG
            controls.IsEnabled = false;
            EStop.Visibility   = Visibility.Visible;

            Task.Run(() =>
                     {
                         try
                         {
                             var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;

                             var fltPos = float.TryParse(App.DataModel.OffsetValue, out float flt)
                                              ? flt
                                              : throw new InvalidCastException();

                             float? vel                                                                       = null;
                             if (float.TryParse(App.DataModel.VelocityValue, out float velf) && velf > 0) vel = velf;

                             if (!festo.move_abs(ref interrupt, fltPos, vel, null))
                                 App.errorManager.set_error((int)DefaultError.MtnMoveFail
                                                          , "Festo_GoTo_Click"
                                                          , (int)DefaultErrorAction.MotionController);

                         } finally
                         {
                             Dispatcher.Invoke(() =>
                                               {
                                                   controls.IsEnabled = true;
                                                   interrupt          = false;
                                                   EStop.Visibility   = Visibility.Collapsed;
                                               });
                         }
                     });
            #endif
        }

        private void stop_immediate_click(object sender, System.Windows.RoutedEventArgs e)
        {
            interrupt = true;
        }

        private void home_click(object sender, RoutedEventArgs e)
        {
            #if !OFFLINE_DEBUG
            controls.IsEnabled = false;

            Task.Run(() =>
                     {
                         try
                         {
                             var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;

                             if (!festo.home())
                                 App.errorManager.set_error((int)DefaultError.MtnHomeFail
                                                          , "Festo"
                                                          , (int)DefaultErrorAction.MotionController);
                         } finally
                         {
                             Dispatcher.Invoke(() =>
                                               {
                                                   controls.IsEnabled = true;
                                               });
                         }
                     });
            #endif
        }

        private void ack_festo_click(object sender, RoutedEventArgs e)
        {
            #if !OFFLINE_DEBUG
            controls.IsEnabled = false;

            Task.Run(() =>
                     {
                         try
                         {
                             var festo = App.devices.device_list.Find(x => x is FestoController) as FestoController;
                             festo.StopFesto();
                         } finally
                         {
                             Dispatcher.Invoke(() =>
                                               {
                                                   controls.IsEnabled = true;
                                               });
                         }
                     });
            #endif
        }
    }
}

namespace Projekt1
{
    public partial class AppDataModel : INotifyPropertyChanged
    {
        private string offsetValue   = "0.0";
        private string velocityValue = "0.0";
        private string fStatus       = "-999";
        private string fDiagMsg      = "-999";
        private string fPosition     = "-999";
        private string fVelocity     = "-999";
        private bool   isStep        = false;
        private bool   isAbs         = true;

        [JsonIgnore]
        public string OffsetValue
        {
            get => offsetValue;
            set
            {
                if (value.EndsWith(".") || string.IsNullOrEmpty(value))
                {
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

        [JsonIgnore]
        public bool IsStep
        {
            get => isStep;
            set
            {
                isStep = value;
                if (isStep && IsAbs) IsAbs = false;
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
}