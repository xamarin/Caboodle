namespace Xamarin.Essentials
{
    public static partial class NumberExtensions
    {
        public static string ToOrdinal(this int num)
        {
            var modedNum = num % 10;
            if (((num / 10) % 10) == 1)
            {
                return $"{num}th";
            }
            switch (modedNum)
            {
                case 1: return $"{num}st";
                case 2: return $"{num}nd";
                case 3: return $"{num}rd";
                default: return $"{num}th";
            }
        }
    }
}
