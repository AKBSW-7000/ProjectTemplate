using System.Windows.Navigation;
using AKBControls;
using AKBUtilities;

namespace Projekt1;
public class FestoController : FestoControl, IObservableDevice
{
    private IerrorManager _emgr;
    public FestoController(IerrorManager errorManager, string filePath, string id) : base(filePath, id)
    {
        _emgr = errorManager;
    }

    public override bool initialize()
    {
#if OFFLINE_DEBUG
        return true;
#else
        return base.initialize();
#endif
    }

    public string getIconURI() => "..\\Resources\\FESTO_Cont.jpg";
    public bool getConnectedStatus() => IsConnected;
}

public interface IObservableDevice : IDevicesInterface
{
    string getIconURI();

    bool getConnectedStatus();

}
