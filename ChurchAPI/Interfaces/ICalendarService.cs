using ChurchAPI.Models;

namespace ChurchAPI.Interfaces
{
    public interface ICalendarService
    {
        List<CalendarEventDTO> Parse(string ics);

        List<CalendarEventDTO> GetEventsBetween(string ics, DateOnly from, DateOnly to);
    }
}
