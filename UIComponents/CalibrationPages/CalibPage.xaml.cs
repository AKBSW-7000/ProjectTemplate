using System.Windows.Controls;

namespace Projekt1;

public partial class CalibPage : UserControl
{
    public CalibPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
