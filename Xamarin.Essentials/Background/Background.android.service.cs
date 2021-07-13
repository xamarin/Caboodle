using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;

namespace Xamarin.Essentials.Background
{
    [Service]
    public class BackgroundService : Service
    {
        static bool isRunning;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!isRunning)
            {
                Background.StartJobs();

                isRunning = true;
            }

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
        }
    }
}
