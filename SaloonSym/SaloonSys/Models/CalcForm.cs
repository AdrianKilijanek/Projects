using System.ComponentModel.DataAnnotations;

namespace SaloonSys.Models
{
    /// <summary>
    /// Model do transferu danych z formularza
    /// </summary>
    public class CalcForm
    {
        [Display(Name = "Kwota kredytu (PLN)")]
        [Range(1000, 10000000, ErrorMessage = "Kwota musi być między 1000 a 10 000 000")]
        public decimal Amount { get; set; }

        [Display(Name = "Roczna stopa procentowa (%)")]
        [Range(0.1, 50, ErrorMessage = "Oprocentowanie musi być między 0,1% a 50%")]
        public decimal InterestRate { get; set; }

        [Display(Name = "Liczba miesięcy")]
        [Range(1, 600, ErrorMessage = "Okres musi być między 1 a 600 miesięcy")]
        public int Months { get; set; }
    }
}
