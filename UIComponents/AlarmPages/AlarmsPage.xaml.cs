using System.Windows.Controls;

namespace Projekt1;
public partial class AlarmsPage : UserControl
{
    public AlarmsPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
