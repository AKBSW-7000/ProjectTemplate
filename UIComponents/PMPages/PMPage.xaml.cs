using System.Windows.Controls;

namespace Projekt1;

public partial class PMPage : UserControl
{
    public PMPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
