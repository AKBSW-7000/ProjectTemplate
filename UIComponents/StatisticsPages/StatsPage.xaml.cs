using System.Windows.Controls;

namespace Projekt1;
public partial class StatsPage : UserControl
{
    public StatsPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
