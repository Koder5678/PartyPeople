using Website.Models;

namespace Website.ViewModels;

public class HomeViewModel
{
    public IEnumerable<Event> Events { get; set; } = Enumerable.Empty<Event>();
    public IEnumerable<EmployeeEventCount> TopColleagues { get; set; } = Enumerable.Empty<EmployeeEventCount>();
    public IEnumerable<Event> EventsWithNoAttendees { get; set; } = Enumerable.Empty<Event>();
}
