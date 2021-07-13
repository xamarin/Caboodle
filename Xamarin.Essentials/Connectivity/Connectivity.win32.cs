using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static void StartListeners() =>
             NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;

        static void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) =>
            OnConnectivityChanged();

        static void StopListeners() =>
             NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;

        static NetworkAccess PlatformNetworkAccess
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable() ? NetworkAccess.Internet : NetworkAccess.None;
            }
        }

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
        {
            get
            {
                foreach (var i in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (i.OperationalStatus == OperationalStatus.Up && i.NetworkInterfaceType != NetworkInterfaceType.Tunnel && i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        ConnectionProfile type;
                        switch (i.NetworkInterfaceType)
                        {
                            case NetworkInterfaceType.Ethernet:
                            case NetworkInterfaceType.Ethernet3Megabit:
                            case NetworkInterfaceType.FastEthernetFx:
                            case NetworkInterfaceType.FastEthernetT:
                                type = ConnectionProfile.Ethernet;
                                break;

                            case NetworkInterfaceType.Wireless80211:
                                type = ConnectionProfile.WiFi;
                                break;

                            case NetworkInterfaceType.Wman:
                            case NetworkInterfaceType.Wwanpp:
                            case NetworkInterfaceType.Wwanpp2:
                                type = ConnectionProfile.Cellular;
                                break;

                            default:
                                type = ConnectionProfile.Unknown;
                                break;
                        }

                        yield return type;
                    }
                }
            }
        }
    }
}
