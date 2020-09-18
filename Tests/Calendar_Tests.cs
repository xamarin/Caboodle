using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Calendar_Tests
    {
        [Fact]
        public async Task Calendar_Get_Calendar_List_Fail_On_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Calendars.GetCalendarsAsync());

        [Fact]
        public async Task Calendar_Get_Event_By_Id_Fail_On_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Calendars.GetEventAsync("An ID"));

        [Fact]
        public async Task Calendar_Get_Events_Fail_On_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Calendars.GetEventsAsync());
    }
}
