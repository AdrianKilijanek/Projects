using SaloonSys.Models;
using System.Threading.Tasks;

namespace SaloonSys.Services
{
    public interface IEmailService
    {
        Task SendAppointmentApprovedAsync(Appointment appointment);
        Task SendAppointmentRejectedAsync(Appointment appointment, string reason);
        Task SendAppointmentChangedAsync(Appointment appointment, DateTime oldStartAt);
        Task SendEmailAsync(string email, string subject, string body);
    }
}