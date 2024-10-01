using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

using WpfAnimatedGif;

namespace Projekt1;

/// <summary>
/// Interaction logic for Splash.xaml
/// </summary>
public partial class Splash : Window, INotifyPropertyChanged
{
    public Splash()
    {
        InitializeComponent();
        Topmost = true;
        ImageBehavior.SetAnimatedSource(gifLogo, gifLogo.Source);
        ImageBehavior.AddAnimationLoadedHandler(gifLogo, loaded);
        ImageBehavior.AddAnimationCompletedHandler(gifLogo, (sender, e) =>
                                                               LogoVisibility = Visibility.Hidden);
    }

    private void loaded(object sender, RoutedEventArgs e)
    {
        ImageBehavior.SetAutoStart(gifLogo, true);
        ImageBehavior.SetRepeatBehavior(gifLogo, new RepeatBehavior(1));
    }

    private double progress = 0.0;

    private Visibility logovis = Visibility.Hidden;
    public double Progress
    {
        get
        {
            return progress;
        }
        set
        {
            progress = value; OnPropertyChanged("Progress");
        }
    }

    public Visibility LogoVisibility
    {
        get
        {
            return logovis;
        }
        set
        {
            logovis = value;
            OnPropertyChanged("LogoVisibility");
            OnPropertyChanged("AltLogoVisibility");
        }
    }

    public Visibility AltLogoVisibility
    {
        get
        {
            return logovis == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
    }

    private string task;
    public string Task
    {
        get
        {
            return task;
        }
        set
        {
            task = value; OnPropertyChanged("Task");
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
    public static string GetLastPart(string fullString)
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
