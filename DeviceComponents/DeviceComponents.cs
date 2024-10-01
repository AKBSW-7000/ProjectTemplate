using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AKBControls;

using festo_mbTcp;

namespace Projekt1;

public class DeviceComponents
{
    #region deviceObjects
    public readonly List<IDevicesInterface> device_list = new ();

    #endregion
    #region constructor
    private DeviceComponents()
    {

    }
    public static DeviceComponents construct_devices()
    {
        DeviceComponents components = new();
        var fp = App.directoryManager.get_directory_path(AKBUtilities.DirectoryType.Settings);
        components.device_list.Add(new FestoController(fp, "1"));
        return components;
    }
    #endregion

    #region Initializer
    public bool Initialize_all_devices()
    {
        try
        {
            bool[] initResult = new bool[(device_list.Count)];
            for (var i = 0; i<device_list.Count;i++)
            {
                initResult[i]=device_list[i].Initialize();
            }
            if(initResult.All(x => x)) 
                return initResult.All(x=>x);
            for (var i = 0; i < initResult.Length; i++)
            {
                if (initResult[i]) continue;
                App.debuggingLog.TraceEvent(TraceEventType.Critical,i, $"@connect_devices_{device_list[i].GetType().Name}");
            }
            return initResult.All(x => x);
        }
        catch { return false; }
    }
    #endregion
}

