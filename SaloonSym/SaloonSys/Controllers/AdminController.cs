using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaloonSys.Data;
using SaloonSys.Helpers;
using SaloonSys.Models;
using SaloonSys.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaloonSys.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Strona główna - Kalendarz
        [HttpGet("Calendar")]
        public IActionResult Calendar()
        {
            return View();
        }

        // API - Pobierz wszystkie rezerwacje (JSON dla kalendarza)
        [HttpGet("api/appointments")]
        public async Task<IActionResult> GetAppointments([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var query = _context.Appointments.AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.StartAt >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.StartAt <= to.Value);

            var appointments = await query.ToListAsync();

            var events = appointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.ClientName} - {a.ServiceType.GetDisplayName()}",
                start = a.StartAt.ToString("yyyy-MM-ddTHH:mm"),
                end = a.StartAt.AddMinutes(a.DurationMinutes).ToString("yyyy-MM-ddTHH:mm"),
                status = a.Status.ToString(),
                clientName = a.ClientName,
                clientEmail = a.ContactEmail,
                clientPhone = a.ContactPhone,
                serviceType = a.ServiceType.GetDisplayName(),
                price = a.Price,
                notes = a.Notes,
                backgroundColor = a.Status switch
                {
                    AppointmentStatus.Pending => "#ffc107",
                    AppointmentStatus.Confirmed => "#28a745",
                    AppointmentStatus.Rejected => "#dc3545",
                    AppointmentStatus.Cancelled => "#6c757d",
                    _ => "#007bff"
                },
                borderColor = a.Status switch
                {
                    AppointmentStatus.Pending => "#ff9800",
                    AppointmentStatus.Confirmed => "#27ae60",
                    AppointmentStatus.Rejected => "#c0392b",
                    AppointmentStatus.Cancelled => "#5a6c7d",
                    _ => "#0056b3"
                }
            }).ToList();

            return Json(events);
        }

        // Zatwierdź rezerwację
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Confirmed;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            // Wyślij email
            await _emailService.SendAppointmentApprovedAsync(appointment);

            return Ok(new { message = "Rezerwacja zatwierdzona!" });
        }

        // Odrzuć rezerwację
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectAppointment(int id, [FromBody] RejectRequest request)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Rejected;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            // Wyślij email
            await _emailService.SendAppointmentRejectedAsync(appointment, request.Reason);

            return Ok(new { message = "Rezerwacja odrzucona!" });
        }

        // Edytuj rezerwację (zatwierdzoną)
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditAppointment(int id, [FromBody] EditAppointmentRequest request)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.Status != AppointmentStatus.Confirmed)
                return BadRequest("Można edytować tylko zatwierdzone rezerwacje");

            var oldStartAt = appointment.StartAt;

            appointment.ClientName = request.ClientName ?? appointment.ClientName;
            appointment.ContactEmail = request.ContactEmail ?? appointment.ContactEmail;
            appointment.ContactPhone = request.ContactPhone ?? appointment.ContactPhone;
            appointment.Notes = request.Notes ?? appointment.Notes;
            appointment.StartAt = request.StartAt ?? appointment.StartAt;
            appointment.DurationMinutes = request.DurationMinutes ?? appointment.DurationMinutes;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            // Wyślij email o zmianie
            await _emailService.SendAppointmentChangedAsync(appointment, oldStartAt);

            return Ok(new { message = "Rezerwacja zaktualizowana!" });
        }

        // Szybka rezerwacja z admina
        [HttpPost("create-quick")]
        public async Task<IActionResult> CreateQuickAppointment([FromBody] CreateQuickAppointmentRequest request)
        {
            // Pobierz czas i cenę dla wybranej usługi
            var (duration, price) = request.ServiceType.GetDetails();

            var appointment = new Appointment
            {
                ClientName = request.ClientName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                ServiceType = request.ServiceType,
                StartAt = request.StartAt,
                DurationMinutes = duration,
                Price = price,
                Status = AppointmentStatus.Confirmed,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                PreferredContact = ContactMethod.Email
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { id = appointment.Id, message = "Rezerwacja dodana!" });
        }

        // Usuń rezerwację
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rezerwacja usunięta!" });
        }
    }

    // Request models
    public class RejectRequest
    {
        public string Reason { get; set; }
    }

    public class EditAppointmentRequest
    {
        public string ClientName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public DateTime? StartAt { get; set; }
        public int? DurationMinutes { get; set; }
        public string Notes { get; set; }
    }

    public class CreateQuickAppointmentRequest
    {
        public string ClientName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public VisitServiceType ServiceType { get; set; }
        public DateTime StartAt { get; set; }
        public string Notes { get; set; }
    }
}
