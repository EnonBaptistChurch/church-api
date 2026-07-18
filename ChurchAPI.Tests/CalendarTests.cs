using ChurchAPI.Models;
using ChurchAPI.Services;
namespace ChurchAPI.Tests;

public class CalendarTests
{
    [Fact]
    public void Should_translate_simple_event_to_json_model()
    {
        // Arrange

        var ics =
                "BEGIN:VCALENDAR\r\n" +
                "VERSION:2.0\r\n" +
                "PRODID:-//Test//EN\r\n" +
                "BEGIN:VEVENT\r\n" +
                "UID:test-event\r\n" +
                "DTSTAMP:20250701T090000Z\r\n" +
                "SUMMARY:Coffee Morning\r\n" +
                "DTSTART;TZID=Europe/London:20250808T090000\r\n" +
                "DTEND;TZID=Europe/London:20250808T103000\r\n" +
                "END:VEVENT\r\n" +
                "END:VCALENDAR\r\n";


        var service = new CalendarService();


        // Act

        var result = service.Parse(ics);


        // Assert

        Assert.Single(result);

        Assert.Equal(
            "Coffee Morning",
            result[0].Title);

        Assert.Equal(
            "09:00",
            result[0].StartTime);

        Assert.Equal(
            "10:30",
            result[0].EndTime);
    }

    [Fact]
    public void Should_not_shift_local_times()
    {
        var ics = 
            "BEGIN:VCALENDAR\r\n" +
            "BEGIN:VEVENT\r\n" +
            "SUMMARY:Coffee Morning\r\n" +
            "DTSTART;TZID=Europe/London:20250808T090000\r\n" +
            "DTEND;TZID=Europe/London:20250808T103000\r\n" +
            "END:VEVENT\r\n" +
            "END:VCALENDAR";


        var service = new CalendarService();


        var result = service.Parse(ics);


        Assert.Equal(
            "09:00",
            result[0].StartTime);
    }

    [Fact]
    public void Should_detect_recurring_events()
    {
        var ics =
                "BEGIN:VCALENDAR\r\n" +
                "BEGIN:VEVENT\r\n" +
                "SUMMARY:Morning Service\r\n" +
                "DTSTART:20250302T111500\r\n" +
                "RRULE:FREQ=WEEKLY;BYDAY=SU\r\n" +
                "END:VEVENT\r\n" +
                "END:VCALENDAR";


        var service = new CalendarService();


        var result = service.Parse(ics);


        Assert.True(result[0].IsRecurring);
    }

    [Fact]
    public void Should_return_events_between_dates_including_recurring_events()
    {
        // Arrange

        var ics =
            "BEGIN:VCALENDAR\r\n" +
            "VERSION:2.0\r\n" +
            "PRODID:-//Test//EN\r\n" +

            // Weekly Sunday service
            "BEGIN:VEVENT\r\n" +
            "UID:sunday-service\r\n" +
            "SUMMARY:Morning Service\r\n" +
            "DTSTART;TZID=Europe/London:20250706T111500\r\n" +
            "DTEND;TZID=Europe/London:20250706T123000\r\n" +
            "RRULE:FREQ=WEEKLY;BYDAY=SU\r\n" +
            "END:VEVENT\r\n" +

            // One-off event
            "BEGIN:VEVENT\r\n" +
            "UID:coffee\r\n" +
            "SUMMARY:Coffee Morning\r\n" +
            "DTSTART;TZID=Europe/London:20250808T090000\r\n" +
            "DTEND;TZID=Europe/London:20250808T103000\r\n" +
            "END:VEVENT\r\n" +

            "END:VCALENDAR\r\n";


        var service = new CalendarService();


        // Act

        var result = service.GetEventsBetween(
            ics,
            new DateOnly(2025, 7, 20),
            new DateOnly(2025, 8, 9));


        // Assert

        Assert.Equal(4, result.Count);


        Assert.Contains(
            result,
            x => x.Title == "Morning Service"
                 && x.Date == new DateOnly(2025, 7, 20));


        Assert.Contains(
            result,
            x => x.Title == "Morning Service"
                 && x.Date == new DateOnly(2025, 7, 27));


        Assert.Contains(
            result,
            x => x.Title == "Morning Service"
                 && x.Date == new DateOnly(2025, 8, 3));


        Assert.Contains(
            result,
            x => x.Title == "Coffee Morning");
    }

    [Fact]
    public void Should_not_return_excluded_recurring_dates()
    {
        var ics =
            "BEGIN:VCALENDAR\r\n" +
        "VERSION:2.0\r\n" +
        "PRODID:-//Test//EN\r\n" +
        "BEGIN:VEVENT\r\n" +
        "UID:sunday-service\r\n" +
        "SUMMARY:Morning Service\r\n" +
        "DTSTART;TZID=Europe/London:20250706T111500\r\n" +
        "DTEND;TZID=Europe/London:20250706T123000\r\n" +
        "RRULE:FREQ=WEEKLY;BYDAY=SU\r\n" +
        "EXDATE;TZID=Europe/London:20250727T111500\r\n" +
        "END:VEVENT\r\n" +
        "END:VCALENDAR";


        var service = new CalendarService();
        var result = service.GetEventsBetween(ics, new DateOnly(2025, 7, 20), new DateOnly(2025, 8, 10));

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result,x => x.Date == new DateOnly(2025, 7, 27));
    }

    [Fact]
    public void Should_remove_duplicate_events()
    {
        var ics =
    @"BEGIN:VCALENDAR\r
VERSION:2.0\r
PRODID:-//Test//EN\r
BEGIN:VEVENT\r
UID:event1\r
SUMMARY:Coffee Morning\r
DTSTART:20250808T090000\r
DTEND:20250808T103000\r
END:VEVENT\r
BEGIN:VEVENT\r
UID:event2\r
SUMMARY:Coffee Morning\r
DTSTART:20250808T090000\r
DTEND:20250808T103000\r
END:VEVENT\r
END:VCALENDAR\r";


        var service = new CalendarService();


        var result = service.GetEventsBetween(
            ics,
            new DateOnly(2025, 8, 1),
            new DateOnly(2025, 8, 31));


        Assert.Single(result);
    }
}
