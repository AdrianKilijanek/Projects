namespace SaloonSys.Models
{
    /// <summary>
    /// Model zwracany z kontrolera - wynik obliczeń
    /// </summary>
    public class CalcResult
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal AnnualPayment { get; set; }
    }
}
