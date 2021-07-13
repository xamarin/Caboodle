using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Xamarin.Essentials.Background
{
    public static class BackgroundService
    {
        static nint taskId;
        static bool isRunning;

        /// <summary>
        /// Start the execution of background service
        /// </summary>
        public static void Start()
        {
            if (isRunning)
                return;

            taskId = UIApplication.SharedApplication.BeginBackgroundTask("BackgroundTask", Stop);
            Background.StartJobs();

            isRunning = true;
        }

        public static void Stop()
        {
            UIApplication.SharedApplication.EndBackgroundTask(taskId);
        }
    }
}
