using System.Threading.Tasks;
using System.Windows.Controls;
using Projekt1.StationProcesses;

namespace Projekt1;

public partial class CalibPage : UserControl
{
    public CalibPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }

    private void festoproc_onCLick(object sender, System.Windows.RoutedEventArgs e)
    {
        IsEnabled = false;

        Task.Run(() =>
                 {
                     try
                     {
                         new FestoProcess().PresserProcess();
                     } finally
                     {
                        Dispatcher.Invoke(() => IsEnabled = true);
                     }

                 });
    }
}
