using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials.Background
{
    public static partial class Background
    {
        static Dictionary<string, IBackgroundTask> schedules = new Dictionary<string, IBackgroundTask>();

        public static void Add<T>(Func<T> schedule)
            where T : IBackgroundTask
        {
#if NETSTANDARD1_0
            var typeName = schedule.GetType().GenericTypeArguments[0]?.Name;
#else
            var typeName = schedule.GetType().GetGenericArguments()[0]?.Name;
#endif

            if (typeName != null && !schedules.ContainsKey(typeName))
                schedules.Add(typeName, schedule());
        }

        internal static Task StartJobs()
        {
            return Task.WhenAll(schedules.Values.Select(x => x.StartJob()));
        }

        public static void StartBackgroundWork()
        {
            PlatformStart();
        }
    }

    public interface IBackgroundTask
    {
        Task StartJob();
    }
}
