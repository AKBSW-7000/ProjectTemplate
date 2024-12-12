using System.Windows.Navigation;
using ACS_Control;
using AKBControls;
using AKBUtilities;

namespace Projekt1;
public class ACSController: AcsInterface, IObservableDevice
{
    public readonly string controllerIP = "127.0.0.1";
    public ACSController(EthernetPrm ethp,int TotalProc, string SettingsPath, IerrorManager err) : base(TotalProc, SettingsPath, err)
    {
        controllerIP = ethp.EthIpAddress;
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
