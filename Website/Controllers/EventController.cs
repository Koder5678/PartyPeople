using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Persistence;
using Website.ViewModels;


namespace Website.Controllers
{
    public class EventController : Controller
    {
        private readonly DbContext _dbContext;
        private readonly IValidator<Event> _validator;
        private readonly ILogger<EventController> _logger;

        public EventController(DbContext dbContext, IValidator<Event> validator, ILogger<EventController> logger)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
        }

        // GET: Event
        public async Task<ActionResult> Index([FromQuery] bool showHistoricEvents = false, CancellationToken cancellationToken = default)
        {
            var events = await _dbContext.Events.GetAllAsync(showHistoricEvents, cancellationToken);
            return View(new EventListViewModel { IsShowingHistoricEvents = showHistoricEvents, Events = events });
        }

        // GET: Event/Details/5
        public async Task<ActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var @event = await _dbContext.Events.GetByIdAsync(id, cancellationToken);
            var employees = await _dbContext.Employees.GetAllAsync(cancellationToken);
            var attendees = await _dbContext.Attendees.GetByEventIdAsync(id, cancellationToken); 

            var viewModel = new EventDetailsViewModel
            {
                Event = @event,
                Employees = employees,
                Attendees = attendees 
            };

            return View(viewModel);
        }



        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event @event, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(@event, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(@event);
            }

            var createdEvent = await _dbContext.Events.CreateAsync(@event, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = createdEvent.Id });
        }

        // GET: Event/Edit/5
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            var @event = await _dbContext.Events.GetByIdAsync(id, cancellationToken);
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Event @event, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(@event, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(@event);
            }

            var updatedEvent = await _dbContext.Events.UpdateAsync(@event, cancellationToken);

            return RedirectToAction(nameof(Details), new { id = updatedEvent.Id });
        }

        // GET: Event/Delete/5
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Events.ExistsAsync(id, cancellationToken);
            if (!exists)
                return NotFound();

            await _dbContext.Events.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }


        // POST: Event/AddAttendee
       [HttpPost]
        public async Task<ActionResult> AddAttendee(int eventId, int employeeId, string preferredDrink, CancellationToken cancellationToken)
        {
            try
            {
                var attendee = new Attendee
                {
                    EventId = eventId,
                    EmployeeId = employeeId,
                    PreferredDrink = preferredDrink
                };

                await _dbContext.Attendees.CreateAsync(attendee, cancellationToken);

               
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // Handle duplicate RSVP 
                Console.WriteLine("This employee has already RSVP'd for this event.");
            }
            catch (Exception ex)
            {
                // Catch unexpected errors 
                Console.WriteLine("An error occurred while adding the RSVP.");
            }

            return RedirectToAction(nameof(Details), new { id = eventId });
        }


    

    }
}
