using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Projekt1;

public partial class MainWindow : Window
{
    public static HomePage home_page;
    public static StatsPage statistics_page;
    public static DevicePage devices_page;
    public static AlarmsPage alarms_page;
    public static SettingsPage settings_page;
    public static CalibPage calib_page;
    public static PMPage pm_page;
    public static ServerPage server_page;

    private ContentControl main_content_controller;
    public MainWindow()
    {
        DataContext = App.DataModel;
        InitializeComponent();
        main_content_controller = new ContentControl() { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
        main_content_controller.Content = home_page = new HomePage();
        statistics_page = new StatsPage();
        devices_page = new DevicePage();
        alarms_page = new AlarmsPage();
        settings_page = new SettingsPage();
        calib_page = new CalibPage();
        pm_page= new PMPage(); 
        server_page = new ServerPage();
        Presentor_Grid.Children.Add(main_content_controller);
        Login.IsVisibleChanged += Login_IsVisibleChanged;
        App.userManager.PropertyChanged += UserManager_PropertyChanged;
        MouseDown += (sender, args) =>
        {
            if (args.ChangedButton == MouseButton.Left)
                try
                {
                    args.Handled = true;
                    DragMove();
                }
                catch (Exception)
                {
                }
        };

    }

    private void UserManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(App.userManager.CurrentLevel))
        {
            deviceBtn.IsEnabled = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default;
            settingsBtn.IsEnabled = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default;
            calibBtn.IsEnabled = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default;
            pmBtn.IsEnabled = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default;
            serverBtn.IsEnabled = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default;
            loginLbl.Visibility = App.userManager.CurrentLevel <= AKBUtilities.UserLevels.Default ? Visibility.Visible: Visibility.Collapsed;
            logoutLbl.Visibility = App.userManager.CurrentLevel > AKBUtilities.UserLevels.Default ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void Login_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
        MainGrid.IsEnabled = Login.Visibility != Visibility.Visible;
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        App.debuggingLog.TraceEvent(TraceEventType.Information, 0, "Application ShutDown.");
        App.Current.Shutdown();
    }

    private void close_window(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cbox = sender as ComboBox;
        if (cbox.SelectedItem is not CultureInfo) return;
        App.DataModel.SelectedCulture = cbox.SelectedItem.ToString();
    }

    private void home_button_click(object sender, RoutedEventArgs e)
    {
        if(home_page is not UserControl) return;
        main_content_controller.Content = home_page;
    }
    private void stats_button_click(object sender, RoutedEventArgs e)
    {
        if (statistics_page is not UserControl) return;
        main_content_controller.Content = statistics_page;
    }
    private void device_button_click(object sender, RoutedEventArgs e)
    {
        if (devices_page is not UserControl) return;
        main_content_controller.Content = devices_page;
    }
    private void alarms_button_click(object sender, RoutedEventArgs e)
    {
        if (alarms_page is not UserControl) return;
        main_content_controller.Content = alarms_page;
    }
    private void settings_button_click(object sender, RoutedEventArgs e)
    {
        if (settings_page is not UserControl) return;
        main_content_controller.Content = settings_page;
    }
    private void calib_button_click(object sender, RoutedEventArgs e)
    {
        if (calib_page is not UserControl) return;
        main_content_controller.Content = calib_page;
    }
    private void server_button_click(object sender, RoutedEventArgs e)
    {
        if (server_page is not UserControl) return;
        main_content_controller.Content = server_page;
    }
    private void PM_button_click(object sender, RoutedEventArgs e)
    {
        if (pm_page is not UserControl) return;
        main_content_controller.Content = pm_page;
    }

    private void CancelLoginClick(object sender, RoutedEventArgs e)
    {
        ((Button)sender).IsEnabled = Login.IsEnabled = false;
        try {
            Login.Visibility = Visibility.Collapsed;
        }
        finally { 
            ((Button)sender).IsEnabled = Login.IsEnabled = true;            
        }
    }

    private void loginUsrClick(object sender, RoutedEventArgs e)
    {
        ((Button)sender).IsEnabled = Login.IsEnabled =false;
        try
        {
            var userName = usTbx.Text;
            var password = pwTbx.Text;
            usTbx.Text = pwTbx.Text = "";
            if (App.userManager.Login(userName, password))
                Login.Visibility = Visibility.Collapsed;
        }
        finally
        {
            ((Button)sender).IsEnabled = Login.IsEnabled = true;
        }
    }

    private void loginOut_Click(object sender, RoutedEventArgs e)
    {
        ((Button)sender).IsEnabled = Login.IsEnabled = false;
        try
        {
            App.userManager.logout();
            Login.Visibility = Visibility.Visible;
        }
        finally
        {
            ((Button)sender).IsEnabled = Login.IsEnabled = true;
        }
    }
}
