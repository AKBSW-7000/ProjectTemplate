using System.Windows.Controls;
using System.Windows;
using Projekt1.UIComponents;
using System.Windows.Media;

namespace Projekt1;
public partial class DevicePage : UserControl
{
    public static FestoDevice festodev;
    public DevicePage()
    {
        InitializeComponent();

        foreach (var IDI in App.devices.device_list)
        {
            DeviceStatusNButton devBtn = new(IDI);
            devBtn.Foreground = App.Current.Resources["fontColor"] as SolidColorBrush;
            devBtn.Click += device_list_selected;
            DeviceListSP.Children.Add(devBtn);
        }
    }

    private void device_list_selected(object sender, RoutedEventArgs e)
    {
        var dsb = sender as DeviceStatusNButton;
        DeviceLabel.Content            = dsb.Text;
        DeviceContentPresenter.Content = null;
        if (dsb.device is FestoController)
        {
            DeviceContentPresenter.Content = festodev == null ? new FestoDevice(): festodev;
#if !DEBUG && !OFFLINE_DEBUG
            if (!dsb.ConnectivityStatus) dsb.device.initialize();
#endif
        }

        expDevices.IsExpanded = false;
    }
}

