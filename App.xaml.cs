using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using System.Windows;

using AKBUtilities;
using AKBControls;
using Splash;
using System.Linq;

namespace Projekt1;

public partial class App : Application
{
    #region private Static Objects
    private static SingleInstanceGuard sig;
    private static Timer               DcTimer;
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
        CommonUse.CONST_DB_NAME = Assembly.GetExecutingAssembly().GetName().Name;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Screen splashScreen = new();
        userManager = new UserManager();
        backup_db(out var stdO, out var stdE);
        init_directories();
        init_errorMgr();
        init_traceLogger();
        init_dataModel();

        debuggingLog.TraceEvent(TraceEventType.Information, 0, "Application Started");
        #if DEBUG
        debuggingLog.TraceEvent(TraceEventType.Information, 0, "In debug mode");
        #else
            splashScreen.Topmost = true;
            splashScreen.Show();
        #endif
        ObservableCollection<Delegate> actions = new();

        actions.Add((Func<bool>)init_devices);
        Task.Factory.StartNew(() =>
                              {
                                  //Splashscreen
                                  splashScreen.init_application(actions);

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
        devices.terminate_all();
        AppDataModel.AppModelSave(directoryManager.get_directory_path(DirectoryType.Settings), ref DataModel);
        alarmStorage.terminate();
        sig.instance_closed();
        App.debuggingLog.TraceEvent(TraceEventType.Information, 0, "Application Exited.");
        base.OnExit(e);
    }
    private void init_directories()
    {
        directoryManager  =  new();
        DcTimer           =  new();
        DcTimer.AutoReset =  true;
        DcTimer.Enabled   =  true;
        DcTimer.Interval  =  TimeSpan.FromDays(1).TotalMilliseconds;
        DcTimer.Elapsed += ((_, _) =>
                            {
                                DcTimer.Stop();
                                foreach (DirectoryType fpt in Enum.GetValues(typeof(DirectoryType)))
                                {
                                    if ((int)fpt < (int)DirectoryType.Results) continue;
                                    DiskCleaner.delete_entries_older_than(directoryManager.get_directory_path(fpt), 30);
                                }
                                DcTimer.Start();
                            });
        foreach (DirectoryType fpt in Enum.GetValues(typeof(DirectoryType)))
        {
            if ((int)fpt < (int)DirectoryType.Results) continue;
            DiskCleaner.delete_entries_older_than(directoryManager.get_directory_path(fpt), 30);
        }
        DcTimer.Start();
    }
    private void init_traceLogger()
    {
        var fp = Assembly.GetExecutingAssembly().GetName().Name;
        debuggingLog        = new TraceSource($"{fp}_logger");
        debuggingLog.Switch = new SourceSwitch("sourceSwitch", "Error");
        debuggingLog.Listeners.Remove("Default");

#region Log file writer init

        directoryManager.create_subdirectory(DirectoryType.Logs, "TraceLog", out var logfp);
        var logFileName = $"{fp}_myListener_{DateTime.Now.ToString("dd-HHmmss-MMyyyy")}.log";
        var filePath    = Path.Combine(logfp, logFileName);
        var tracer      = File.AppendText(filePath);
        tracer.AutoFlush = true;
        
        TextWriterTraceListener textListener = new(logFileName);
        textListener.Filter             = new EventTypeFilter(SourceLevels.All);
        textListener.Name               = "TextWriter";
        textListener.Writer             = tracer;
        textListener.TraceOutputOptions = TraceOptions.DateTime;
        debuggingLog.Listeners.Add(textListener);

#endregion

        #if DEBUG

#region Console debugger init

        //create Console debugger
        ConsoleTraceListener console = new (false);
        console.Filter             = new EventTypeFilter(SourceLevels.All);
        console.TraceOutputOptions = TraceOptions.DateTime;
        console.Name               = "console";
        debuggingLog.Listeners.Add(console);

#endregion

        #endif
        // Allow the trace source to send messages to
        // listeners for all event types. 
        debuggingLog.Switch.Level = SourceLevels.All;

        Current.DispatcherUnhandledException += (_, e) =>
                                                {
                                                    debuggingLog.TraceEvent(TraceEventType.Error
                                                                          , e.Exception.HResult
                                                                          , @$"
===========DispatcherUnhandledException==============

TimeStamp  = {DateTime.Now:yyyy_MM_dd_HH_mm_ss_fff}
StackTrace = 
{e.Exception.StackTrace}

===========DispatcherUnhandledException==============
");
                                                    debuggingLog.Flush();
                                                    e.Handled = true;
                                                };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                                                      {
                                                          Exception ex = (Exception)e.ExceptionObject;

                                                          debuggingLog.TraceEvent(e.IsTerminating?TraceEventType.Critical:TraceEventType.Error
                                                            , ex.HResult
                                                            , @$"
=================UnhandledException=================

TimeStamp     = {DateTime.Now:yyyy_MM_dd_HH_mm_ss_fff}
IsTerminating = {e.IsTerminating}
StackTrace    = 
{ex.StackTrace}

=================UnhandledException=================
"); 
                                                          debuggingLog.Flush();
                                                      };
        errorManager.errorEventHandler += (_, e) => App.debuggingLog.TraceEvent(TraceEventType.Error, e.LastErrorCode, e.LastErrorMessage);

    }
    private void init_errorMgr()
    {
        errorManager = new Projekt1ErrManager(directoryManager.get_directory_path(DirectoryType.Settings));
        alarmStorage = new (errorManager);
        alarmStorage.initialize(SaveTarget.Db);
    }
    private void init_dataModel()
    {
        DataModel = AppDataModel.AppModelConstructor(directoryManager.get_directory_path(DirectoryType.Settings));
    }
    private bool init_devices()
    {
        devices = DeviceComponents.construct_devices();
        return devices.Initialize_all_devices();
    }
    private bool backup_db(out string stdOutput, out string stdError)
    {
        var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Dbase");

        var latestSqlFile = Directory.GetFiles(dirPath, "*.sql")
                                     .Select(file => new FileInfo(file))
                                     .OrderByDescending(fileInfo => fileInfo.CreationTime)
                                     .FirstOrDefault();

        if (latestSqlFile != null)
        {
            var diff = DateTime.Now - latestSqlFile.CreationTime;

            if (diff.TotalDays < 7)
            {
                stdOutput = "File is newer than Database_Backup_Interval setting of {} days";
                stdError  = "";
                return true;
            }
        }

        var pi = new ProcessStartInfo();
        pi.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Dbase", "SQL_backup.bat");
        stdOutput = stdError = "";
        if (!File.Exists(pi.FileName))
        {
            stdOutput = "Error";
            stdError = "Batch file or directory doesn't exist!";
            return false;
        }
        pi.WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "Dbase");
        pi.UseShellExecute = false;
        pi.CreateNoWindow = true;
        // *** Redirect the output ***
        pi.RedirectStandardError = true;
        pi.RedirectStandardOutput = true;

        if (!FindMysqldumpPath(out var fp))
        {
            stdOutput = "Error";
            stdError = "mysqldump.exe cannot be found.";
            return false;
        }
        pi.Arguments = $"{CommonUse.CONST_DB_ADMIN} {CommonUse.CONST_DB_PASSWORD} \"{fp}\"";

        var p = Process.Start(pi);
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            stdError = p.StandardError.ReadToEnd();
            stdOutput = p.StandardOutput.ReadToEnd();
            return false;
        }
        return true;
    }

    private bool FindMysqldumpPath(out string fp)
    {
        // Check common directories
        string[] commonPaths = {
                                       @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
                                       @"C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
                                   };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
            {
                fp = path;
                return true;
            }
        }

        // Check PATH environment variable
        var paths = Environment.GetEnvironmentVariable("PATH").Split(';');
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, "mysqldump.exe");
            if (File.Exists(fullPath))
            {
                fp = path;
                return true;
            }
        }

        fp = "";
        return false;
    }

}
