using System.Windows.Navigation;
using ACS_Control;
using AKBUtilities;

namespace Projekt1;
public class ACSController: AcsInterface, IObservableDevice
{
    private string controllerIP = "127.0.0.1";
    public ACSController(string hostIP,int TotalProc, string SettingsPath, IerrorManager err) : base(TotalProc, SettingsPath, err)
    {
        controllerIP = hostIP;
    }
    public bool initialize()
    {
#if DEBUG || OFFLINE_DEBUG
        return base.Connect("127.0.0.1", true);
#endif
        return base.Connect(controllerIP, false);
    }

    public void terminate()
    {
        Disconnect();
    }

    public bool   getConnectedStatus() => isConnected;
    public string getIconURI()         => "..\\Resources\\ACSCont.jpg";
}
