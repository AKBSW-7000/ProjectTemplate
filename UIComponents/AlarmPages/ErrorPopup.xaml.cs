using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AKBUtilities;
using Material.Icons;
using Material.Icons.WPF;

namespace Projekt1;

public partial class ErrorPopup : UserControl
{
    private Point CurrentPoint;
    private Point originalPoint;

    public int ErrorCount
    {
        get
        {
            return (int)GetValue(ErrorCountProperty);
        }
        set
        {
            SetValue(ErrorCountProperty, value);
        }
    }

    // Using a DependencyProperty as the backing store for ErrorCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ErrorCountProperty =
        DependencyProperty.Register("ErrorCount", typeof(int), typeof(ErrorPopup), new PropertyMetadata(0));


    public ErrorPopup()
    {
        InitializeComponent();
#region  Drag Move Functions
        RenderTransform =  new TranslateTransform(0, 0);
        RenderTransformOrigin             =  new Point(0, 0);
        //AlarmListView.MouseLeftButtonDown += (_, e) =>
        //                                               {
        //                                                   CurrentPoint = e.GetPosition(Parent as UIElement);
        //                                                   CaptureMouse();
        //                                               };
        
        MouseLeftButtonDown += (_, e) =>
                               {
                                   CurrentPoint = e.GetPosition(Parent as UIElement);
                                   CaptureMouse();
                               };

        MouseLeftButtonUp += (_, e) =>
                             {
                                 if (IsMouseCaptured) ReleaseMouseCapture();
                             };


        MouseMove += (_, e) =>
                     {
                         var V = e.GetPosition(Parent as UIElement) - CurrentPoint;
                         if (IsMouseCaptured && V.Length>1)
                         {
                             (RenderTransform as TranslateTransform).X =
                                 e.GetPosition(Parent as UIElement).X - ActualWidth / 2;

                             (RenderTransform as TranslateTransform).Y =
                                 e.GetPosition(Parent as UIElement).Y - ActualHeight / 2;
                         }

                         e.Handled = true;
                     };
#endregion
        Loaded += (_, _) =>
                  {
                      App.errorManager.errorEventHandler += ErrorManager_errorEventHandler;
                      Dispatcher.BeginInvoke(() => update_alarmListView());
                  };

        MouseDoubleClick += ErrorPopup_MouseDoubleClick;
        
    }

    private void ErrorManager_errorEventHandler(object sender, AKBUtilities.ErrorEventArg e)
    {
        Dispatcher.BeginInvoke(() => update_alarmListView());
        App.debuggingLog.TraceEvent(TraceEventType.Error,e.LastErrorCode,e.LastErrorMessage);
    }

    private void ErrorPopup_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ErrorBox.Visibility == Visibility.Collapsed)
        {
            ErrorBox.Visibility                       = Visibility.Visible;
        }
        else
        {
            ErrorBox.Visibility                       = Visibility.Collapsed;
            (RenderTransform as TranslateTransform).X =  (RenderTransform as TranslateTransform).Y = 0;
        }
    }

    private void update_alarmListView()
    {
        AlarmListView.Items.Clear();
        ErrorCount = App.alarmStorage.get_alarm_count();
        if (ErrorCount < 1)
        {
            Visibility = ErrorBox.Visibility = Visibility.Collapsed;
            return;
        }

        var ENUMERABLE = App.alarmStorage.alarm.ToList();
        foreach (var ac in ENUMERABLE)
        {
            alarmLabel al = new(ac)
            {
                Content = $"[{ac.timeOccurred:dd-MMM-yy HH:mm:ss}] {ac.message}",
                FontSize = 24
                               ,
                Padding = new Thickness(-10)
            };

            var _contextMenu = new ContextMenu()
            {
                AllowDrop = true,
            };

            var resetClickitem = new MenuItem()
            {
                Header = "Reset",
                FontSize = 24,
                Height = 50
            };

            resetClickitem.Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.Restore,
                Height = 20,
                Width = 20,
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };
            resetClickitem.Click += (_, e) =>
            {
                int index = App.alarmStorage.alarm.Select(x => x.id).ToList().IndexOf(ac.id);
                App.alarmStorage.resolve_alarm_at(index, "Admin Manual Reset");
                (al.Parent as ListView).Items.Remove(al);
                ErrorCount = App.alarmStorage.get_alarm_count();
                if (ErrorCount < 1)
                {
                    Visibility = ErrorBox.Visibility = Visibility.Collapsed;
                }
            };

            _contextMenu.Items.Add(resetClickitem);
            al.ContextMenu = _contextMenu;
            AlarmListView.Items.Insert(0, al);

        }

        if (ErrorCount > 0)
        {
            Visibility = ErrorBox.Visibility = Visibility.Visible;
        }
      
    }

    private void reset_all_click(object sender, RoutedEventArgs e)
    {
        App.alarmStorage.resolve_all_alarms();
        update_alarmListView();
    }
}

public class alarmLabel: Label{
    private readonly AlarmStorage.AlarmComponent alarm;
    public alarmLabel(AlarmStorage.AlarmComponent ac) : base()
    {
        alarm = ac;
    }

}