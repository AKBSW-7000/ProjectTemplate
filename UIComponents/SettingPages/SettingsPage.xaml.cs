using System.Windows.Controls;

namespace Projekt1;
public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
