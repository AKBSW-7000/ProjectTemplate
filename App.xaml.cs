﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AKBUtilities;

namespace Projekt1;

public partial class App : Application
{
    #region private Static Objects
    private static SingleInstanceGuard sig;
    private static Splash splashScreen;
    #endregion
    #region public Static Objects
    public static TraceSource debuggingLog;
    public static IerrorManager errorManager;
    public static DirectoryManager directoryManager;
    public static UserManager userManager;
    public static DeviceComponents devices;
    public static AlarmStorage alarmStorage;
    public static AppDataModel DataModel;
    #endregion
    public App()
    {
        sig = new SingleInstanceGuard();
        if (!sig.no_instance_running())
        {
            MessageBox.Show("Multiple Instance of Application detected!");
            Current.Shutdown();
        }
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        splashScreen = new Splash();
        directoryManager = new DirectoryManager();
        userManager = new UserManager();

        init_traceLogger();
        init_errorMgr();
        init_dataModel();
       
        devices = DeviceComponents.construct_devices();
        
        debuggingLog.TraceEvent(TraceEventType.Information, 0, "Application Started");
#if DEBUG
        debuggingLog.TraceEvent(TraceEventType.Information, 0, "In debug mode");
#else
            splashScreen.Topmost = true;
            splashScreen.Show();
#endif
        ObservableCollection<Delegate> actions = new ();
        
        actions.Add((Func<bool>)devices.Initialize_all_devices);

        Task.Factory.StartNew(() =>
        {
            foreach (Delegate action in actions)
            {
                splashScreen.Task = $"{GetLastPart(action.Target.ToString())} {action.Method.Name}";
                Thread td = new Thread((() =>
                {
                    if (action is Func<bool> actionWithoutReturnValue)
                    {
                        actionWithoutReturnValue();
                        splashScreen.Progress += (100 / actions.Count);
                    }

                }))
                {
                    Name = $"{action.Method.Name}_thread"
                };
                td.Start();
                if (td.IsAlive) td.Join(360000);
            }

            //to create and show the main window
            Dispatcher.Invoke(() =>
            {
                //initialize the main window, set it as the application main window
                //and close the splash screen
                Current.MainWindow = new MainWindow();
                Current.MainWindow.Show();
#if !DEBUG
                    splashScreen.Close();
#endif
            });
        });
    }
    protected override void OnExit(ExitEventArgs e)
    {
        var filePath = Path.Combine(directoryManager.get_directory_path(DirectoryType.Settings));
        AppDataModel.AppModelSave(directoryManager.get_directory_path(DirectoryType.Settings), ref DataModel);
        sig.instance_closed();
        base.OnExit(e);
    }
    private void init_traceLogger()
    {
        var fp = Assembly.GetExecutingAssembly().GetName().Name;
        
        debuggingLog = new TraceSource($"{fp}_logger");
        debuggingLog.Switch = new SourceSwitch("sourceSwitch", "Error");
        debuggingLog.Listeners.Remove("Default");

#region Log file writer init
        var logFileName = $"{fp}_myListener_{DateTime.Now.ToString("dd-HHmmss-MMyyyy")}.log";
        TextWriterTraceListener textListener = new TextWriterTraceListener(logFileName);
        textListener.Filter = new EventTypeFilter(SourceLevels.All);
        textListener.Name = "TextWriter";
        var filePath = Path.Combine(directoryManager.get_directory_path(DirectoryType.Logs),logFileName);
        StreamWriter tracer = File.AppendText(filePath);
        tracer.AutoFlush = true;
        textListener.Writer = tracer;
        textListener.TraceOutputOptions = TraceOptions.DateTime;
        debuggingLog.Listeners.Add(textListener);
        #endregion
#if DEBUG
#region Console debugger init
        //create Console debugger
        ConsoleTraceListener console = new ConsoleTraceListener(false);
        console.Filter = new EventTypeFilter(SourceLevels.All);
        console.TraceOutputOptions = TraceOptions.DateTime;
        console.Name = "console";            
        debuggingLog.Listeners.Add(console);
#endregion
#endif
        // Allow the trace source to send messages to
        // listeners for all event types. 
        debuggingLog.Switch.Level = SourceLevels.All;
        return;
    }
    private void init_errorMgr()
    {
        errorManager = new Projekt1ErrManager(directoryManager.get_directory_path(DirectoryType.Settings));
    }
    private void init_dataModel()
    {
        DataModel = AppDataModel.AppModelConstructor(directoryManager.get_directory_path(DirectoryType.Settings));
    }

    private void init_devices()
    {
        var fp = directoryManager.get_directory_path(DirectoryType.Settings);
    }

    private string GetLastPart(string fullString)
    {
        int lastDotIndex = fullString.LastIndexOf('.');

        if (lastDotIndex != -1 && lastDotIndex < fullString.Length - 1)
        {
            // Extract the substring after the last dot
            return fullString.Substring(lastDotIndex + 1);
        }
        else
        {
            // Return the original string if no dot is found or if it's the last character
            return fullString;
        }
    }

}
