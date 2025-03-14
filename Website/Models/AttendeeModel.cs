namespace Website.Models;

public class Attendee
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int EmployeeId { get; set; }
    public string? PreferredDrink { get; set; }
}
