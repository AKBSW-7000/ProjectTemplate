using System;
using System.ComponentModel;

using AKBUtilities;

namespace Projekt1;
public class UserManager:INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string property)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
    private UserLevels currentLevel = UserLevels.Default;
    public UserLevels CurrentLevel
    {
        get => currentLevel; 
        private set{
            currentLevel = value;
            RaisePropertyChanged(nameof(CurrentLevel));
        }
    } 

    public UserManager()
    {
    
    }

    public bool Login(string username, string password)
    {
#if DEBUG
        CurrentLevel = UserLevels.Admin;
        return true;
#endif
        try
        {
            return true;
        }
        catch (Exception ex) { }
        return false;
    }

    public void logout()
    {
        CurrentLevel = UserLevels.Default;
    }
}
