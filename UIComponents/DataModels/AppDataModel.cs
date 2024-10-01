using Newtonsoft.Json;

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Projekt1;
public partial class AppDataModel : INotifyPropertyChanged
{
    #region private Fields
    [JsonIgnore]
    private const string fileName = @"Application_Data_Model.json";
    #endregion

    #region Public Properties

    #endregion

    #region PublicEvent
    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string property)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
    #endregion //PublicEvent

    #region constructor
    public AppDataModel()
    {
    }
    public static AppDataModel AppModelConstructor(string filePath)
    {
        var file = Path.Combine(filePath, fileName);
        if (!File.Exists(file))
        {
            App.debuggingLog.TraceEvent(TraceEventType.Warning, 0, "Application datamodel file does not exist!");
            var newAppDataModel = new AppDataModel();
            newAppDataModel.AllLang = new Dictionary<CultureInfo, displayWording>();
            newAppDataModel.AllLang.Add(CultureInfo.GetCultureInfo("en-US"), new displayWording());
            newAppDataModel.SelectedCulture = "en-US";
            return newAppDataModel;
        }
        try
        {
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (var tReader = new StreamReader(fileStream))
            using (var jreader = new JsonTextReader(tReader))
            {
                var jSerializer = new JsonSerializer();
                var newAppDataModel = jSerializer.Deserialize<AppDataModel>(jreader);
                if (newAppDataModel is AppDataModel) return newAppDataModel;
            }
        }
        catch { }
        App.debuggingLog.TraceEvent(TraceEventType.Warning, 0, "JsonDeserialization of Application datamodel file failed!");
        return new AppDataModel();
    }
    #endregion

    #region Save and Dispose
    public static void AppModelSave(string filePath, ref AppDataModel apm)
    {
        var file = Path.Combine(filePath, fileName);
        try
        {
            using (var fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var tWriter = new StreamWriter(fileStream))
            using (var jWriter = new JsonTextWriter(tWriter))
            {
                jWriter.Formatting = Formatting.Indented;                 
                jWriter.Indentation = 4;
                var jSerializer = new JsonSerializer();
                jSerializer.Serialize(jWriter, apm);
                apm = null;
            }
        }
        catch {
            App.debuggingLog.TraceEvent(TraceEventType.Warning, 0, "JsonSerialization of Application datamodel file failed!");
        }
    }
    #endregion
}

