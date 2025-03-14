using Website.Models;

namespace Website.ViewModels;

public class EventDetailsViewModel
{
    public Event Event { get; set; } = null!;
    public IEnumerable<Employee> Employees { get; set; } = Enumerable.Empty<Employee>();

    public IEnumerable<Attendee> Attendees { get; set; } = Enumerable.Empty<Attendee>();

}
