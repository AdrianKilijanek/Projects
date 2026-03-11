using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SaloonSys.Models
{
    /// <summary>
    /// Encja reprezentująca rezerwację wizyty u fryzjera
    /// </summary>
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Typ wizyty (enum zamiast string - bezpieczniej)
        /// </summary>
        [Required]
        public VisitServiceType ServiceType { get; set; }

        /// <summary>
        /// Czas trwania w minutach - AUTO z ServiceType
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Cena - AUTO z ServiceType
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Imię i nazwisko klienta (dla niezalogowanych)
        /// </summary>
        [Column(TypeName = "varchar(100)")]
        public string? ClientName { get; set; }

        /// <summary>
        /// Email kontaktowy
        /// </summary>
        [EmailAddress]
        [Column(TypeName = "varchar(100)")]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Telefon kontaktowy
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Preferowana forma kontaktu
        /// </summary>
        public ContactMethod PreferredContact { get; set; } = ContactMethod.Email;

        /// <summary>
        /// Uwagi klienta (opcjonalnie)
        /// </summary>
        [Column(TypeName = "varchar(500)")]
        public string? Notes { get; set; }

        /// <summary>
        /// Status rezerwacji
        /// </summary>
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        /// <summary>
        /// ID użytkownika (jeśli zalogowany)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Powiązanie do użytkownika - ✅ ZMIENIONE NA ApplicationUser
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Kiedy została stworzona rezerwacja
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Enum typów usług z predefiniowanymi czasami i cenami
    /// </summary>
    public enum VisitServiceType
    {
        [Display(Name = "Strzyżenie", Description = "30 minut | 59 PLN")]
        Haircut = 1,

        [Display(Name = "Koloryzacja", Description = "60 minut | 120 PLN")]
        Coloring = 2,

        [Display(Name = "Trwała ondulacja", Description = "90 minut | 150 PLN")]
        Perm = 3,

        [Display(Name = "Stylizacja", Description = "45 minut | 85 PLN")]
        Styling = 4,

        [Display(Name = "Terapia włosów", Description = "30 minut | 45 PLN")]
        HairTherapy = 5
    }

    /// <summary>
    /// Enum preferowanej formy kontaktu
    /// </summary>
    public enum ContactMethod
    {
        [Display(Name = "Email")]
        Email = 1,

        [Display(Name = "Telefon")]
        Phone = 2,

        [Display(Name = "SMS")]
        SMS = 3
    }

    /// <summary>
    /// Enum statusów rezerwacji
    /// </summary>
    public enum AppointmentStatus
    {
        Pending = 0,
        Confirmed = 1,
        Rejected = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Helper do uzyskania czasu i ceny z ServiceType
    /// </summary>
    public static class VisitServiceExtensions
    {
        public static (int Duration, decimal Price) GetDetails(this VisitServiceType type)
        {
            return type switch
            {
                VisitServiceType.Haircut => (30, 59m),
                VisitServiceType.Coloring => (60, 120m),
                VisitServiceType.Perm => (90, 150m),
                VisitServiceType.Styling => (45, 85m),
                VisitServiceType.HairTherapy => (30, 45m),
                _ => (30, 0m)
            };
        }
    }
}
