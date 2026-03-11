using SaloonSys.Helpers;
using SaloonSys.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SaloonSys.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        // Konstruktor - tutaj ustawiamy dane do łączenia z serwerem SMTP
        public EmailService(string smtpServer, int smtpPort, string senderEmail, string senderPassword)
        {
            _smtpServer = smtpServer;      // np. "smtp.gmail.com"
            _smtpPort = smtpPort;          // np. 587
            _senderEmail = senderEmail;    // email, z którego wysyłamy
            _senderPassword = senderPassword; // hasło/token aplikacji
        }

        // Wysłanie emaila o potwierdzeniu rezerwacji
        public async Task SendAppointmentApprovedAsync(Appointment appointment)
        {
            var subject = "Twoja rezerwacja została potwierdzona!";
            var body = $@"
Cześć {appointment.ClientName},

Twoja rezerwacja została zatwierdzona!

Data: {appointment.StartAt:dd.MM.yyyy HH:mm}
Usługa: {appointment.ServiceType.GetDisplayName()}
Cena: {appointment.Price:0.00} PLN
Czas trwania: {appointment.DurationMinutes} minut

Zapraszamy!

Pozdrawiamy,
Fryzjernia";

            await SendEmailAsync(appointment.ContactEmail, subject, body);
        }

        // Wysłanie emaila o odrzuceniu rezerwacji
        public async Task SendAppointmentRejectedAsync(Appointment appointment, string reason)
        {
            var subject = "Twoja rezerwacja została odrzucona";
            var body = $@"
Cześć {appointment.ClientName},

Niestety, Twoja rezerwacja na dzień {appointment.StartAt:dd.MM.yyyy HH:mm} została odrzucona.

Powód: {reason}

Możesz spróbować zarezerwować inny termin.

Pozdrawiamy,
Fryzjernia";

            await SendEmailAsync(appointment.ContactEmail, subject, body);
        }

        // Wysłanie emaila o zmianie rezerwacji
        public async Task SendAppointmentChangedAsync(Appointment appointment, DateTime oldStartAt)
        {
            var subject = "Twoja rezerwacja została zmieniona";
            var body = $@"
Cześć {appointment.ClientName},

Twoja rezerwacja została zmieniona.

Stara data: {oldStartAt:dd.MM.yyyy HH:mm}
Nowa data: {appointment.StartAt:dd.MM.yyyy HH:mm}

Usługa: {appointment.ServiceType.GetDisplayName()}
Cena: {appointment.Price:0.00} PLN

Zapraszamy!

Pozdrawiamy,
Fryzjernia";

            await SendEmailAsync(appointment.ContactEmail, subject, body);
        }

        // Prywatna metoda - tutaj faktycznie wysyłamy email
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var message = new MailMessage(_senderEmail, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };

                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd przy wysyłaniu emaila: {ex.Message}");
            }
        }

    }
}
