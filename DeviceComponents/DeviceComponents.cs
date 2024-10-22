using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AKBControls;
using AKBUtilities;

namespace Projekt1;

public class DeviceComponents
{
    #region deviceObjects
    public readonly List<IObservableDevice> device_list;

    #endregion
    #region constructor
    private DeviceComponents()
    {
        device_list = new();
    }
    public static DeviceComponents construct_devices()
    {
        DeviceComponents components = new();
        var fp = App.directoryManager.get_directory_path(AKBUtilities.DirectoryType.Settings);
        components.device_list.Add(new FestoController(App.errorManager,fp, "1"));
        components.device_list.Add(new ACSController("127.0.0.1",0,fp, App.errorManager));
        return components;
    }
    #endregion

    #region Initializer

    public bool Initialize_all_devices()
    {
        bool[] initResult = new bool[(device_list.Count)];

        for (var i = 0; i < device_list.Count; i++)
        {
            initResult[i] = device_list[i].initialize();
        }

        if (initResult.All(x => x)) return initResult.All(x => x);

        for (var i = 0; i < initResult.Length; i++)
        {
            if (initResult[i]) continue;
            if(device_list[i].GetType().Name is "FestoController")
                App.errorManager.set_error((int)DefaultError.MtnInitFail, "", (int)DefaultErrorAction.ControlPc);
            App.debuggingLog.TraceEvent(TraceEventType.Error
                                      , i
                                      , $"@connect_devices_{device_list[i].GetType().Name}");
        }

        return initResult.All(x => x);
    }

    public object get_device_ofType(Type type)
    {
        if(device_list.Any(x => x.GetType().Name == type.Name)) 
            return device_list.First(x => x.GetType().Name == type.Name);
        return null;
    }
    public IEnumerable<object> get_devices_ofType(Type type)
    {
        if (device_list.Any(x => x.GetType().Name == type.Name))
            return device_list.Where(x => x.GetType().Name == type.Name);
        return null;
    }
    #endregion
}

