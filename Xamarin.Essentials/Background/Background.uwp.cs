using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;

namespace Xamarin.Essentials.Background
{
    public static partial class Background
    {
        const string backServiceName = "BackgroundService";

        internal static void PlatformStart()
        {
            Task.Run(StartBackgroundServiceAsync);
        }

        public static async Task OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();
            await StartJobs();
            deferral.Complete();
        }

        static async Task StartBackgroundServiceAsync()
        {
            var access = await BackgroundExecutionManager.RequestAccessAsync();

            switch (access)
            {
                case BackgroundAccessStatus.Unspecified:
                case BackgroundAccessStatus.DeniedByUser:
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                    await StartJobs();
                    return;
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = backServiceName,
                IsNetworkRequested = true
            };

            var trigger = new ApplicationTrigger();
            builder.SetTrigger(trigger);

            var isAlreadyRegistered = BackgroundTaskRegistration.AllTasks.Any(t => t.Value?.Name == backServiceName);
            if (isAlreadyRegistered)
            {
                foreach (var tsk in BackgroundTaskRegistration.AllTasks)
                {
                    if (tsk.Value.Name == backServiceName)
                    {
                        tsk.Value.Unregister(true);
                        break;
                    }
                }
            }

            builder.Register();
            await trigger.RequestAsync();
        }
    }
}
