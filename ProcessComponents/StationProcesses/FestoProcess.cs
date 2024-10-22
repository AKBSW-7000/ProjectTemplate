using System;
using System.Threading;
using System.Threading.Tasks;
using AKBUtilities;

namespace Projekt1.StationProcesses;
public class FestoProcess
{
    private FestoController festoCtr;
    private ACSController   acsCtr;
    private IerrorManager   errorManager;
    public FestoProcess()
    {
        festoCtr     = App.devices.get_device_ofType(typeof(FestoController)) as FestoController;
        acsCtr       = App.devices.get_device_ofType(typeof(ACSController)) as ACSController;
        errorManager = App.errorManager;
    }

    public bool PresserProcess()
    {
        if (!festoCtr.IsConnected) //check festo connection
            return errorManager.set_error((int)DefaultError.MtnConnFail
                                        , "Festo Not Connected!"
                                        , (int)DefaultErrorAction.MotionController);
        if (festoCtr.Malfunction) //check festo malfunctions
            return errorManager.set_error((int)DefaultError.CritMtnStop
                                        , "Axis Malfunction active"
                                        , (int)DefaultErrorAction.MotionController);
        if (!acsCtr.isConnected) //Check acs is connected
            return errorManager.set_error((int)DefaultError.MtnConnFail
                                        , "Acs Not Connected!"
                                        , (int)DefaultErrorAction.MotionController);

        var interrupt  = false;
        var monitoring = true;
        var  param      = festoCtr.Param;
        if (!float.TryParse(param.SafePos,out var safePos))
            return errorManager.set_error((int)DefaultError.Msc
                                        , "Safe Position not in correct format!"
                                        , (int)DefaultErrorAction.ControlPc);
        if (!float.TryParse(param.ContactPos, out var conPos))
            return errorManager.set_error((int)DefaultError.Msc
                                        , "Contact Position not in correct format!"
                                        , (int)DefaultErrorAction.ControlPc);
        if (!float.TryParse(param.EndPos, out var endPos))
            return errorManager.set_error((int)DefaultError.Msc
                                        , "End Position not in correct format!"
                                        , (int)DefaultErrorAction.ControlPc);
        
        var monitoringThread = new Thread(() =>
                                          {
                                              var dtn = DateTime.Now;
                                              while (monitoring && !interrupt)
                                              {
                                                  Thread.Sleep(1);
                                                  interrupt = !acsCtr.ReadInput((int)InputIO.FestoOverPressed
                                                                                  , out int res)
                                                                  ? true
                                                                  : res == 1;

                                                  if ((DateTime.Now - dtn).TotalSeconds > 6) interrupt = true;
                                              }
                                          });
        monitoringThread.Start();

        //Move to safe if not inSafePos
        if (!festoCtr.InSafePos)
        {
            monitoring = festoCtr.move_abs(ref interrupt, safePos, null, null);
            if (!monitoring)
            {
                monitoringThread.Join();
                return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                            , "Error Moving to Safe Position"
                                            , (int)DefaultErrorAction.ControlPc);
            }
        }   
        if(!festoCtr.InSafePos)
        {
            monitoring = false;
            monitoringThread.Join();
            return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                        , "Error Moving to Safe Position"
                                        , (int)DefaultErrorAction.ControlPc);
        }

        //move to contact position
        monitoring = festoCtr.move_abs(ref interrupt, conPos, null, null);
        if (!monitoring)
        {
            monitoringThread.Join();
            interrupt  = false;
            festoCtr.move_abs(ref interrupt, safePos, null, null);
            return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                        , "Error Moving to Contact Position"
                                        , (int)DefaultErrorAction.ControlPc);
        }

        //move to end position
        monitoring = festoCtr.move_abs(ref interrupt, endPos, null, null);
        if (!monitoring)
        {
            monitoringThread.Join();
            interrupt = false;
            festoCtr.move_abs(ref interrupt, safePos, null, null);
            return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                        , "Error Moving to End Position"
                                        , (int)DefaultErrorAction.ControlPc);
        }

        //Move to safe Position
        monitoring = festoCtr.move_abs(ref interrupt, safePos, null, null);
        if (!monitoring)
        {
            monitoringThread.Join();
            return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                                                 , "Error Moving to Safe Position"
                                                                 , (int)DefaultErrorAction.ControlPc);
        } 
        monitoring = false;
        monitoringThread.Join();

        if (!festoCtr.InSafePos)
        {
            return errorManager.set_error((int)DefaultError.MtnMoveZFail
                                        , "Error Moving to Safe Position"
                                        , (int)DefaultErrorAction.ControlPc);
        }

        return errorManager.normal();
    }

}
