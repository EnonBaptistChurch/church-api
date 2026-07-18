namespace ChurchAPI.Models
{
    public record CalendarEventDTO
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateOnly Date { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public bool IsRecurring { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}
