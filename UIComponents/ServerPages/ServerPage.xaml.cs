using System.Windows.Controls;

namespace Projekt1;
public partial class ServerPage : UserControl
{
    public ServerPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
    }
}
