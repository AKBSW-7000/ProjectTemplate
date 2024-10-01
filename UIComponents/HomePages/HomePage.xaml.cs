using System.Windows.Controls;

namespace Projekt1;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
