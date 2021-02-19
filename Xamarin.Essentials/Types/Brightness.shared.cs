using System;
using System.Globalization;
#if WINDOWS_UWP
using UwpBrightness = Windows.Graphics.Display.BrightnessOverride;
#endif

namespace Xamarin.Essentials
{
    public readonly partial struct BrightnessOverride : IDisposable, IEquatable<BrightnessOverride>
    {
        internal BrightnessOverride(Brightness oldBrightness, Brightness appliedBrightness)
        {
            OldBrightness = oldBrightness;
            AppliedBrightness = appliedBrightness;
#if WINDOWS_UWP
            @override = null;
#endif
        }

#if WINDOWS_UWP
        readonly UwpBrightness @override;

        public BrightnessOverride(Brightness oldBrightness, Brightness newBrightness, UwpBrightness @override)
            : this(oldBrightness, newBrightness)
        {
            this.@override = @override;
        }

        public void Dispose()
        {
            @override.StopOverride();
        }

        public bool Equals(BrightnessOverride other)
        {
            return OldBrightness.Equals(other.OldBrightness) && AppliedBrightness.Equals(other.AppliedBrightness) && @override.Equals(other.@override);
        }

        public override int GetHashCode()
        {
            return (OldBrightness, AppliedBrightness, @override).GetHashCode();
        }
#endif

        public static bool operator ==(BrightnessOverride lhs, BrightnessOverride rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BrightnessOverride lhs, BrightnessOverride rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            return other is BrightnessOverride b && Equals(b);
        }

        public Brightness OldBrightness { get; }

        public Brightness AppliedBrightness { get; }

#if !WINDOWS_UWP
        public void Dispose()
        {
            AppInfo.SetBrightness(OldBrightness);
        }
#endif

#if !WINDOWS_UWP
        public bool Equals(BrightnessOverride other)
        {
            return OldBrightness.Equals(other.OldBrightness) && AppliedBrightness.Equals(other.AppliedBrightness);
        }

        public override int GetHashCode()
        {
            return (OldBrightness, AppliedBrightness).GetHashCode();
        }
#endif
    }

    public readonly struct Brightness : IEquatable<Brightness>
    {
        public double Value { get; }

        public Brightness(double value)
        {
            const double minValue = 0d;
#if __ANDROID__
            const double minValue = -1d;
#endif
            if (value < minValue || value > 1d)
                throw new ArgumentException("Value has to be between (0,1) or less than one on Android", nameof(value));
            Value = value;
        }

        public static bool operator ==(Brightness lhs, Brightness rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Brightness lhs, Brightness rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            return other is Brightness b && Equals(b);
        }

        public bool Equals(Brightness other)
        {
            return Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value.ToString("F", CultureInfo.InvariantCulture)}";
        }
    }
}
