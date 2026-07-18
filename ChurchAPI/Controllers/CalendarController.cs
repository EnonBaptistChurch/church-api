using ChurchAPI.Interfaces;
using ChurchAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChurchAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalendarController(ICalendarService calendarService) : ControllerBase
    {
        private readonly ICalendarService _calendarService = calendarService;

        [HttpGet(Name = "GetCalendarEvents")]
        public async Task<IEnumerable<CalendarEventDTO>> Get()
        {
            var calendarIcsLocation = "https://calendar.google.com/calendar/ical/enonbcwebsite%40gmail.com/public/basic.ics";
            var calendarRaw = await  new HttpClient().GetAsync(calendarIcsLocation);
            string calendarRawString = await calendarRaw.Content.ReadAsStringAsync();
            return _calendarService.Parse(calendarRawString);
        }

        [HttpGet("range", Name = "GetCalendarEventsInRange")]
        public async Task<IEnumerable<CalendarEventDTO>> GetFromRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            var calendarIcsLocation = "https://calendar.google.com/calendar/ical/enonbcwebsite%40gmail.com/public/basic.ics";
            var calendarRaw = await new HttpClient().GetAsync(calendarIcsLocation);
            string calendarRawString = await calendarRaw.Content.ReadAsStringAsync();
            return _calendarService.GetEventsBetween(calendarRawString,from, to);
        }
    }
}
