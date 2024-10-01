using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json;

namespace Projekt1;
public partial class AppDataModel : INotifyPropertyChanged
{
    #region Language Fields
    private string selectedCulture;
    private Dictionary<CultureInfo, displayWording> allLang =new();
    #endregion
    #region Public Properties
    #region Language Properties
    [JsonIgnore]
    public displayWording SelectedLang
    {
        get;
        set;
    }
    public Dictionary<CultureInfo, displayWording> AllLang
    {
        get => allLang;
        private set
        {
            allLang = value;
            RaisePropertyChanged(nameof(AllLang));
        }

    }
    public string SelectedCulture
    {
        get => selectedCulture;
        set
        {            
            if (value is not null && AllLang.ContainsKey(CultureInfo.GetCultureInfo(value)))
            {
                selectedCulture = value;
                SelectedLang = AllLang[CultureInfo.GetCultureInfo(value)];
                RaisePropertyChanged(nameof(SelectedCulture));
                RaisePropertyChanged(nameof(SelectedLang));
            }
        }
    }

    public string[] AvailLangs => AllLang?.Keys.Select(x => x.ToString()).ToArray();
    #endregion
    #endregion

}
public struct displayWording
{
    public CultureInfo Culture
    {
        get; set;
    }
    public string LoginBtn_Text
    {
        get; set;
    }
    public string LogoutBtn_Text
    {
        get; set;
    }
    
    public string HomeBtn_Text
    {
        get; set;
    }
    public string StatsBtn_Text
    {
        get; set;
    }
    public string DevicesBtn_Text
    {
        get; set;
    }
    public string AlarmsBtn_Text
    {
        get; set;
    }
    public string SettingsBtn_Text
    {
        get; set;
    }
    public string CalibBtn_Text
    {
        get; set;
    }
    public string PMBtn_Text
    {
        get; set;
    }
    public string ServerBtn_Text
    {
        get; set;
    }
    public string Languages_Text
    {
        get; set;
    }
    public string Festo_Pg_StatusLbl
    {
        get;set;
    }
    public string Festo_Pg_DiagMsgLbl
    {
        get; set;
    }
    public string Festo_Pg_PositionLbl
    {
        get; set;
    }
    public string Festo_Pg_DistanceLbl
    {
        get; set;
    }
    
    public string Festo_Pg_VelocityLbl
    {
        get; set;
    }
    public string Festo_Pg_StepLbl
    {
        get; set;
    }
    public string Festo_Pg_AbsoluteLbl
    {
        get; set;
    }
    public string Festo_Pg_Title
    {
        get; set;
    }
    public string LoginWin_UsrNameLbl
    {
        get; set;
    }
    public string LoginWin_PasswordLbl
    {
        get; set;
    }
    public string CancelBtnTxt
    {
        get; set;
    }
    public displayWording()
    {
        Culture = new CultureInfo("en-US");
        LoginBtn_Text = "Login";
        LogoutBtn_Text = "Log out";
        HomeBtn_Text = "Home";
        StatsBtn_Text = "Stats";
        DevicesBtn_Text = "Devices";
        AlarmsBtn_Text = "Alarms";
        Languages_Text = "Languages: ";
        SettingsBtn_Text = "Settings";
        CalibBtn_Text = "Calib";
        ServerBtn_Text = "Server";
        PMBtn_Text = "Maint";
        Festo_Pg_StatusLbl = "Status:";
        Festo_Pg_DiagMsgLbl = "Diagnostic Message:";
        Festo_Pg_PositionLbl = "Position (mm):";
        Festo_Pg_DistanceLbl = "Distance (mm):";
        Festo_Pg_VelocityLbl = "Velocity (mm/s):";
        Festo_Pg_StepLbl = "Step";
        Festo_Pg_AbsoluteLbl = "Absolute";
        Festo_Pg_Title= "Festo Motor Control";
        LoginWin_UsrNameLbl="Username:";
        LoginWin_PasswordLbl = "Password:";
        CancelBtnTxt = "Cancel";
    }
}
