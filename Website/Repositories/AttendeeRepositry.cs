using Dapper;
using System.Data;
using Website.Models;
using Website.Persistence;

namespace Website.Repositories;

/// <summary>
/// Repository for accessing attendees from a database.
/// </summary>
public class AttendeeRepository : RepositoryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttendeeRepository"/> class.
    /// </summary>
    /// <param name="connectionProvider">The connection provider to use.</param>
    public AttendeeRepository(IDbConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    /// <summary>
    /// Creates the Attendees table if it doesn't already exist.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An awaitable task.</returns>
    public async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            @"
                CREATE TABLE IF NOT EXISTS [Attendees] (
                    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                    [EventId] INTEGER NOT NULL,
                    [EmployeeId] INTEGER NOT NULL,
                    [PreferredDrink] TEXT NULL,
                    FOREIGN KEY ([EventId]) REFERENCES [Event]([Id]) ON DELETE CASCADE,
                    FOREIGN KEY ([EmployeeId]) REFERENCES [Employee]([Id]) ON DELETE CASCADE,
                    UNIQUE ([EventId], [EmployeeId])
                );
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Adds a new attendee.
    /// </summary>
    /// <param name="attendee">The attendee to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created attendee with assigned ID.</returns>
    public async Task<Attendee> CreateAsync(Attendee attendee, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                INSERT INTO [Attendees] ([EventId], [EmployeeId], [PreferredDrink])
                VALUES (@EventId, @EmployeeId, @PreferredDrink);

                SELECT [Id], [EventId], [EmployeeId], [PreferredDrink]
                FROM [Attendees]
                WHERE [Id] = last_insert_rowid();
            ",
            parameters: new
            {
                attendee.EventId,
                attendee.EmployeeId,
                attendee.PreferredDrink
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.QuerySingleAsync<Attendee>(command);
    }

    /// <summary>
    /// Gets attendees for a specific event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of attendees for the event.</returns>
    public async Task<IEnumerable<Attendee>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT [Id], [EventId], [EmployeeId], [PreferredDrink]
                FROM [Attendees]
                WHERE [EventId] = @EventId;
            ",
            parameters: new { EventId = eventId },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.QueryAsync<Attendee>(command);
    }

    public async Task<IEnumerable<EmployeeEventCount>> GetTop5MostSocialColleaguesAsync(CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            @"
                SELECT 
                    E.Id,
                    E.FirstName,
                    E.LastName,
                    COUNT(A.Id) AS EventCount
                FROM 
                    Employee E
                LEFT JOIN 
                    Attendees A ON A.EmployeeId = E.Id
                GROUP BY 
                    E.Id, E.FirstName, E.LastName
                ORDER BY 
                    EventCount DESC
                LIMIT 5;
            ",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        return await Connection.QueryAsync<EmployeeEventCount>(command);
    }


}



