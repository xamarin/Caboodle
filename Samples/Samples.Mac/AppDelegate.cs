using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Samples.Mac
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : FormsApplicationDelegate
    {
        static App formsApp;

        NSWindow window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var screenSize = NSScreen.MainScreen.Frame.Size;
            var rect = new CGRect(0, 0, 1024, 768);
            rect.Offset((screenSize.Width - rect.Width) / 2, (screenSize.Height - rect.Height) / 2);

            window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Xamarin.Essentials",
                TitleVisibility = NSWindowTitleVisibility.Hidden,
            };

            // Add Quit shortcut
            var appMenubar = new NSMenu();
            var appMenuItem = new NSMenuItem();
            appMenubar.AddItem(appMenuItem);

            var appMenu = new NSMenu();
            appMenuItem.Submenu = appMenu;

            var quitMenuItem = new NSMenuItem("Quit", "q", delegate
                {
                    NSApplication.SharedApplication.Terminate(appMenubar);
                });
            appMenu.AddItem(quitMenuItem);

            NSApplication.SharedApplication.MainMenu = appMenubar;
        }

        public override NSWindow MainWindow => window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            LoadApplication(formsApp ??= new App());

            base.DidFinishLaunching(notification);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
    }
}
