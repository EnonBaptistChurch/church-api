using ChurchAPI.Models;
using System.Globalization;
using Ical.Net;
using Ical.Net.Serialization;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using ChurchAPI.Interfaces;

namespace ChurchAPI.Services
{
    public class CalendarService : ICalendarService
    {
        public List<CalendarEventDTO> Parse(string ics)
        {
            if (string.IsNullOrWhiteSpace(ics)) return [];

            var serializer = new CalendarSerializer();

            // First attempt: parse the ICS as provided
            var calendars = CalendarCollection.Load(ics);

            var calendar = calendars.FirstOrDefault();

            // If deserialization failed, try a more tolerant approach by injecting minimal required headers
            if (calendar == null)
            {
                var fixedIcs = ics;
                if (!fixedIcs.Contains("VERSION:"))
                    fixedIcs = fixedIcs.Replace("BEGIN:VCALENDAR", "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//EnonBC//EN");

                calendar = serializer.Deserialize(new StringReader(fixedIcs)) as Ical.Net.Calendar;
            }

            if (calendar == null) throw new InvalidOperationException("Failed to deserialize calendar.");

            return [.. calendar.Events.Select(MapEvent)];
        }

        public List<CalendarEventDTO> GetEventsBetween(string ics, DateOnly from, DateOnly to)
        {
            var calendars = CalendarCollection.Load(ics);
            var calendar = calendars.First();

            var fromDate = new CalDateTime(from.Year, from.Month, from.Day);
            var toDate = new CalDateTime(to.Year, to.Month, to.Day);

            var results = new List<CalendarEventDTO>();
            foreach (var item in calendar.Events)
            {
                var occurrences = item.GetOccurrences(fromDate, new EvaluationOptions())
                                        .TakeWhile(x => x.Period.StartTime.Value <= to.ToDateTime(TimeOnly.MaxValue));
                foreach (var occurrence in occurrences)
                {
                    var start = GetUKDateTime(occurrence.Period.StartTime);

                    if (DateOnly.FromDateTime(start) > to)
                        continue;

                    var endTime = occurrence.Period.EndTime ?? occurrence.Period.EffectiveEndTime;
                    results.Add(new CalendarEventDTO
                    {
                        Id = item.Uid ?? "",
                        Description = item.Description ?? "",
                        Title = item.Summary ?? "",
                        Date = DateOnly.FromDateTime(start),
                        StartTime = start.ToString("HH:mm"),
                        EndTime = GetUKDateTime(endTime!).ToString("HH:mm") ?? "",
                        IsRecurring = item.RecurrenceRule != null
                    });
                }

                foreach (var exception in item.ExceptionDates.GetAllDates()
                    .Where(exceptionDate => exceptionDate > fromDate && exceptionDate <= toDate))
                {

                    var cancelledDate = DateOnly.FromDateTime(exception.Value);

                    if (cancelledDate >= from &&
                        cancelledDate <= to)
                    {
                        if (item.Summary.Contains("Bible Study") && cancelledDate.DayOfWeek == DayOfWeek.Wednesday)
                            continue;
                        results.Add(new CalendarEventDTO
                        {
                            Id = item.Uid?? "",
                            Description = item.Description ?? "",
                            Title = item.Summary ?? "",
                            StartTime = GetUKDateTime(exception).ToString("HH:mm"),
                            Date = cancelledDate,

                            Status = "Cancelled"
                        });
                    }
                }
            }


            return [.. results
                        .GroupBy(x => new
                            {
                                x.Title,
                                x.Date,
                                x.StartTime,
                                x.EndTime
                            })
                        .Select(x => x.First())
                        .OrderBy(x => x.Date)
                        .ThenBy(x => x.StartTime)];
        }

        private static DateTime GetUKDateTime(CalDateTime dateTime)
        {
            if (!dateTime.IsUtc) 
                return dateTime.Value;

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value,tz);
        }

        private CalendarEventDTO MapEvent(Ical.Net.CalendarComponents.CalendarEvent item)
        {
            var start = GetUKDateTime(item.DtStart);

            return new CalendarEventDTO
            {
                Title = item.Summary ?? "",
                Date = DateOnly.FromDateTime(start),
                StartTime = start.ToString("HH:mm"),
                EndTime = GetUKDateTime(item.DtEnd).ToString("HH:mm") ?? "",
                IsRecurring = item.RecurrenceRule != null,
            };
        }
    }
}
