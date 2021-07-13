using System;
using System.Runtime.InteropServices;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static void StartEnergySaverListeners()
        {
        }

        static void StopEnergySaverListeners()
        {
        }

        static void StartBatteryListeners()
        {
        }

        static void StopBatteryListeners()
        {
        }

        static double PlatformChargeLevel
        {
            get
            {
                NativeMethods.GetSystemPowerStatus(out var powerStatus);

                if (powerStatus.BatteryLifePercent == 255)
                    return 1.0;

                return powerStatus.BatteryLifePercent / 100d;
            }
        }

        static BatteryState PlatformState
        {
            get
            {
                NativeMethods.GetSystemPowerStatus(out var powerStatus);

                switch (powerStatus.BatteryFlag)
                {
                    case NativeMethods.BatteryFlag.Charging:
                        return BatteryState.Charging;
                    case NativeMethods.BatteryFlag.High:
                    case NativeMethods.BatteryFlag.Low:
                    case NativeMethods.BatteryFlag.Critical:
                        if (ChargeLevel >= 1.0)
                            return BatteryState.Full;
                        return BatteryState.Discharging;
                    case NativeMethods.BatteryFlag.NoBattery:
                        return BatteryState.NotPresent;
                }

                if (ChargeLevel >= 1.0)
                    return BatteryState.Full;

                return BatteryState.Unknown;
            }
        }

        static BatteryPowerSource PlatformPowerSource
        {
            get
            {
                NativeMethods.GetSystemPowerStatus(out var powerStatus);
                switch (powerStatus.ACLineStatus)
                {
                    case NativeMethods.ACLine.Online:
                        return BatteryPowerSource.AC;
                    case NativeMethods.ACLine.Offline:
                    case NativeMethods.ACLine.BackupPower:
                        return BatteryPowerSource.Battery;
                    default:
                        return BatteryPowerSource.Unknown;
                }
            }
        }

        static EnergySaverStatus PlatformEnergySaverStatus
        {
            get
            {
                NativeMethods.GetSystemPowerStatus(out var powerStatus);
                return powerStatus.SystemStatusFlag == 1 ? EnergySaverStatus.On : EnergySaverStatus.Off;
            }
        }

        static class NativeMethods
        {
            [DllImport("Kernel32", ExactSpelling = true)]
            internal static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS systemPowerStatus);

            [StructLayout(LayoutKind.Sequential)]
            internal struct SYSTEM_POWER_STATUS
            {
                [MarshalAs(UnmanagedType.U1)]
                public ACLine ACLineStatus;
                [MarshalAs(UnmanagedType.U1)]
                public BatteryFlag BatteryFlag;
                public byte BatteryLifePercent;
                public byte SystemStatusFlag;            // set to 0 prior to Win10
                public int BatteryLifeTime;
                public int BatteryFullLifeTime;
            }

            internal enum ACLine : byte
            {
                Offline = 0x00,
                Online = 0x01,
                BackupPower = 0x02,
                Unknown = 0xFF,
            }

            [Flags]
            internal enum BatteryFlag : byte
            {
                High = 0x01,
                Low = 0x02,
                Critical = 0x04,
                Charging = 0x08,
                NoBattery = 0x80,
                Unknown = 0xFF,
            }
        }
    }
}
