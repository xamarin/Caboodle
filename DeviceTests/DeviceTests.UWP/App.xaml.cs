using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graphics.Canvas.Effects;
using UnitTests.HeadlessRunner;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xunit.Runners.UI;

namespace DeviceTests.UWP
{
    public sealed partial class App : RunnerApplication
    {
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            // Ensure the current window is active
            Window.Current.Activate();

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args;
                if (!string.IsNullOrEmpty(protocolArgs?.Uri?.Host))
                {
                    var parts = protocolArgs.Uri.Host.Split('_');
                    if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]))
                    {
                        var ip = parts[0]?.Replace('-', '.');

                        if (int.TryParse(parts[1], out var port))
                        {
                            await Tests.RunAsync(new TestOptions
                            {
                                Assemblies = new List<Assembly> { typeof(Battery_Tests).Assembly },
                                NetworkLogHost = ip,
                                NetworkLogPort = port,
                                Filters = Traits.GetCommonTraits(),
                                Format = TestResultsFormat.XunitV2
                            });
                        }
                    }
                }
            }
        }

        protected override void OnInitializeRunner()
        {
            AddTestAssembly(typeof(App).GetTypeInfo().Assembly);
            AddTestAssembly(typeof(Accelerometer_Tests).GetTypeInfo().Assembly);
        }
    }
}
