using System;
using System.Management;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        static string GetModel()
        {
            var managementClass = new ManagementClass("Win32_ComputerSystem");
            foreach (var managementObject in managementClass.GetInstances())
            {
                var model = (string)managementObject["Model"];
                return string.IsNullOrEmpty(model) ? null : model;
            }
            return string.Empty;
        }

        static string GetManufacturer()
        {
            var managementClass = new ManagementClass("Win32_ComputerSystem");
            foreach (var managementObject in managementClass.GetInstances())
            {
                var manufacturer = (string)managementObject["Manufacturer"];
                return string.IsNullOrEmpty(manufacturer) ? null : manufacturer;
            }
            return string.Empty;
        }

        static string GetDeviceName() => Environment.MachineName;

        static string GetVersionString() => Environment.OSVersion.VersionString;

        static DevicePlatform GetPlatform() => DevicePlatform.WindowsDesktop;

        static DeviceIdiom GetIdiom() => DeviceIdiom.Desktop;

        static DeviceType GetDeviceType() => DeviceType.Unknown;
    }
}
