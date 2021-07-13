namespace Xamarin.Essentials.Background
{
    public static partial class Background
    {
        internal static void PlatformStart() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
