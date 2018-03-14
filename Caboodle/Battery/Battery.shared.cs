﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
        static event BatteryChangedEventHandler BatteryChanagedInternal;

        static double currentLevel;

        static BatteryPowerSource currentSource;

        static BatteryState currentState;

        public static event BatteryChangedEventHandler BatteryChanged
        {
            add
            {
                var wasRunning = BatteryChanagedInternal != null;

                BatteryChanagedInternal += value;

                if (!wasRunning && BatteryChanagedInternal != null)
                {
                    SetCurrent();
                    StartBatteryListeners();
                }
            }

            remove
            {
                var wasRunning = BatteryChanagedInternal != null;

                BatteryChanagedInternal -= value;

                if (wasRunning && BatteryChanagedInternal == null)
                    StopBatteryListeners();
            }
        }

        static void SetCurrent()
        {
            currentLevel = Battery.ChargeLevel;
            currentSource = Battery.PowerSource;
            currentState = Battery.State;
        }

        static void OnBatteryChanged(double level, BatteryState state, BatteryPowerSource source)
            => OnBatteryChanged(new BatteryChangedEventArgs(level, state, source));

        static void OnBatteryChanged()
            => OnBatteryChanged(ChargeLevel, State, PowerSource);

        static void OnBatteryChanged(BatteryChangedEventArgs e)
        {
            if (currentLevel != e.ChargeLevel ||
                currentSource != e.PowerSource ||
                currentState != e.State)
            {
                SetCurrent();
                BatteryChanagedInternal?.Invoke(e);
            }
        }
    }

    public enum BatteryState
    {
        Unknown,
        Charging,
        Discharging,
        Full,
        NotCharging
    }

    public enum BatteryPowerSource
    {
        Unknown,
        Battery,
        Ac,
        Usb,
        Wireless
    }

    public delegate void BatteryChangedEventHandler(BatteryChangedEventArgs e);

    public class BatteryChangedEventArgs : EventArgs
    {
        internal BatteryChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
        {
            ChargeLevel = level;
            State = state;
            PowerSource = source;
        }

        public double ChargeLevel { get; }

        public BatteryState State { get; }

        public BatteryPowerSource PowerSource { get; }
    }
}
