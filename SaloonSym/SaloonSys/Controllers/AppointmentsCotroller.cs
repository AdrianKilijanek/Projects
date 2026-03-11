using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SaloonSys.Data;
using SaloonSys.Models;

namespace SaloonSys.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Appointment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartAt,ServiceType,ClientName,ContactEmail,ContactPhone,PreferredContact,Notes")] Appointment appointment)
        {
            if (appointment.StartAt < DateTime.Now)
            {
                ModelState.AddModelError("StartAt", "Nie możesz zarezerwować termin w przeszłości");
                return View(appointment);
            }

            if (appointment.ServiceType == 0)
            {
                ModelState.AddModelError("ServiceType", "Wybierz typ usługi");
                return View(appointment);
            }

            if (string.IsNullOrWhiteSpace(appointment.ClientName) && !User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("ClientName", "Wpisz imię i nazwisko");
                return View(appointment);
            }

            if (string.IsNullOrWhiteSpace(appointment.ContactEmail))
            {
                ModelState.AddModelError("ContactEmail", "Wpisz email");
                return View(appointment);
            }

            if (string.IsNullOrWhiteSpace(appointment.ContactPhone))
            {
                ModelState.AddModelError("ContactPhone", "Wpisz numer telefonu");
                return View(appointment);
            }

            var (duration, price) = appointment.ServiceType.GetDetails();
            appointment.DurationMinutes = duration;
            appointment.Price = price;

            var conflictingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.StartAt < appointment.StartAt.AddMinutes(appointment.DurationMinutes) &&
                    a.StartAt.AddMinutes(a.DurationMinutes) > appointment.StartAt &&
                    a.Status != AppointmentStatus.Rejected &&
                    a.Status != AppointmentStatus.Cancelled
                );

            if (conflictingAppointment != null)
            {
                ModelState.AddModelError("StartAt", "Ten termin jest już zarezerwowany");
                return View(appointment);
            }

            if (User.Identity.IsAuthenticated)
            {
                appointment.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(appointment.ClientName))
                {
                    appointment.ClientName = User.Identity.Name ?? "Zalogowany użytkownik";
                }
            }

            appointment.Status = AppointmentStatus.Pending;
            appointment.CreatedAt = DateTime.UtcNow;

            _context.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IQueryable<Appointment> query = _context.Appointments;

            if (User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                query = query.Where(a => a.UserId == userId);
            }

            var appointments = await query
                .OrderBy(a => a.StartAt)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (appointment.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (appointment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                ModelState.AddModelError("", "Ta rezerwacja jest już anulowana");
                return View(appointment);
            }

            appointment.Status = AppointmentStatus.Cancelled;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/api/appointments/available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var selectedDate))
                {
                    return BadRequest(new { error = "Nieprawidłowy format daty" });
                }

                if (selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    return Ok(new { available = new List<string>() });
                }

                var slots = new List<string>();

                for (int hour = 9; hour < 17; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 15)
                    {
                        var slotStart = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);
                        var slotEnd = slotStart.AddMinutes(30);

                        var exists = await _context.Appointments
                            .AnyAsync(a =>
                                a.StartAt < slotEnd &&
                                a.StartAt.AddMinutes(a.DurationMinutes) > slotStart &&
                                a.Status != AppointmentStatus.Rejected &&
                                a.Status != AppointmentStatus.Cancelled
                            );

                        if (!exists)
                        {
                            slots.Add($"{hour:D2}:{minute:D2}");
                        }
                    }
                }

                return Ok(new { available = slots });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
