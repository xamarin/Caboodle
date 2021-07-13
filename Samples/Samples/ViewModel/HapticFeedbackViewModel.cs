using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class HapticFeedbackViewModel : BaseViewModel
    {
        bool isSupported = true;
        HapticFeedbackGenerator generator;

        public HapticFeedbackViewModel()
        {
            ClickCommand = new Command(
                () => ActionInvoke(
                    () => HapticFeedback.Perform(HapticFeedbackType.Click)));
            LongPressCommand = new Command(
                () => ActionInvoke(
                    () => HapticFeedback.Perform(HapticFeedbackType.LongPress)));

            PrepareCommand = new Command(() => ActionInvoke(OnPrepare, string.Empty));
            DisposeCommand = new Command(() => ActionInvoke(OnDispose, string.Empty));
            PerformCommand = new Command(() => ActionInvoke(OnPerform, string.Empty));
        }

        public ICommand ClickCommand { get; }

        public ICommand LongPressCommand { get; }

        public ICommand PrepareCommand { get; }

        public ICommand DisposeCommand { get; }

        public ICommand PerformCommand { get; }

        public bool IsSupported
        {
            get => isSupported;
            set => SetProperty(ref isSupported, value);
        }

        void OnPrepare()
        {
            OnDispose();
            generator = HapticFeedback.PrepareGenerator(HapticFeedbackType.LongPress);
        }

        void OnDispose() => generator?.Dispose();

        void OnPerform() => generator.Perform();

        void ActionInvoke(Action action, string messagePrefix = "Unable to HapticFeedback:")
        {
            try
            {
                action?.Invoke();
            }
            catch (FeatureNotSupportedException)
            {
                IsSupported = false;
            }
            catch (Exception ex)
            {
                DisplayAlertAsync($"{messagePrefix} {ex.Message}");
            }
        }
    }
}
