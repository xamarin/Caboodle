using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class AppInfoViewModel : BaseViewModel
    {
        public string AppPackageName => AppInfo.PackageName;

        public string AppName => AppInfo.Name;

        public string AppVersion => AppInfo.VersionString;

        public string AppBuild => AppInfo.BuildString;

        public string AppTheme => AppInfo.RequestedTheme.ToString();

        public Command ShowSettingsUICommand { get; }

        public Command SetBrightnessCommand { get; }

        public string Brightness => AppInfo.CurrentBrightness.Value.ToString();

        double newBrightness;

        public double NewBrightness
        {
            get => newBrightness;
            set
            {
                if (value > 1)
                    SetProperty(ref newBrightness, 1);
                else if (value < 0)
                    SetProperty(ref newBrightness, 0);
                else
                    SetProperty(ref newBrightness, value);
            }
        }

        public string BrightnessString
        {
            get => NewBrightness.ToString();
            set
            {
                if (double.TryParse(value, out var result))
                    NewBrightness = result;
            }
        }

        public AppInfoViewModel()
        {
            ShowSettingsUICommand = new Command(() => AppInfo.ShowSettingsUI());
            SetBrightnessCommand = new Command(() => AppInfo.SetBrightness(new Brightness(NewBrightness)));
            NewBrightness = AppInfo.CurrentBrightness.Value;
        }
    }
}
