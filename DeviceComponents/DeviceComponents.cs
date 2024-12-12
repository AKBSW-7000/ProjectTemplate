using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AKBControls;
using AKBUtilities;
using Newtonsoft.Json;

namespace Projekt1;

public class DeviceComponents
{
    #region deviceObjects
    public readonly  List<IObservableDevice>    device_list;
    public Dictionary<string, string> connection_Dict;
    #endregion
    #region constructor
    private DeviceComponents()
    {
        device_list     = new();
        connection_Dict = new();
    }
    public static DeviceComponents construct_devices()
    {
        DeviceComponents components = new();
        var fp = App.directoryManager.get_directory_path(DirectoryType.Settings);
        components.device_list.Add(new FestoController(App.errorManager,fp, "1"));
        if(!get_connection_details(fp, out var connectionDict))
            return components;

        components.connection_Dict = connectionDict;
        var ep = JsonConvert.DeserializeObject<EthernetPrm>(components.connection_Dict[nameof(ACSController)]);
        components.device_list.Add(new ACSController(ep, 0,fp, App.errorManager));

        return components;
    }
    #endregion

    #region Initializer

    public bool Initialize_all_devices()
    {
        bool[] initResult = new bool[device_list.Count];

        Parallel.For(0, device_list.Count, (i) => initResult[i] = device_list[i].initialize());

        if (initResult.All(x => x)) return initResult.All(x => x);

        for (var i = 0; i < initResult.Length; i++)
        {
            if (initResult[i]) continue;
            if (device_list[i].GetType().Name is "FestoController")
                App.errorManager.set_error((int)DefaultError.MtnInitFail, $"@connect_devices_{device_list[i].GetType().Name}", (int)DefaultErrorAction.ControlPc);
        }

        return initResult.All(x => x);
    }

    public void terminate_all()
    {
        var directory = App.directoryManager.get_directory_path(DirectoryType.Settings);
        var fp = Path.Combine(directory, "ConnectionParam.json");
        save_connection_details(fp, connection_Dict);

        foreach (var iod in device_list)
        {
            iod.terminate();
        }
    }

    public static bool get_connection_details(string directory, out Dictionary<string, string> details)
    {
        details = new ();

        try
        {
            var fp = Path.Combine(directory, "ConnectionParam.json");

            if (!File.Exists(fp)) return details.Count > 0;

            using FileStream     stream = new(fp, FileMode.Open);
            using StreamReader   sr     = new(stream);
            using JsonTextReader jtr    = new(sr);

            details = new JsonSerializer().Deserialize<Dictionary<string, string>>(jtr);

            return details.Count > 0;
        } catch (Exception ex)
        {
            return App.errorManager.set_error((int)DefaultError.MscExcept
                                            , ex.Message
                                            , (int)DefaultErrorAction.ControlPc);
        }
    }
    public static bool save_connection_details(string filePath, Dictionary<string, string> details)
    {
        try
        {
            if (details is null) return App.errorManager.set_error((int)DefaultError.MscUndef
                                                                 , "Empty connection details"
                                                                 , (int)DefaultErrorAction.ControlPc);

            using FileStream     stream = new(filePath, FileMode.OpenOrCreate);
            using StreamWriter   sw     = new(stream);
            using JsonTextWriter jtr    = new(sw);
            new JsonSerializer().Serialize(jtr, details);

            return App.errorManager.normal();
        } catch (Exception ex)
        {
            return App.errorManager.set_error((int)DefaultError.MscExcept
                                            , ex.Message
                                            , (int)DefaultErrorAction.ControlPc);

        }
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

