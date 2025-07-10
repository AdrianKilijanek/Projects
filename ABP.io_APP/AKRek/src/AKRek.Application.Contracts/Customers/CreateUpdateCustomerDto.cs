using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKRek.Customers;

public class CreateUpdateCustomerDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string SurName { get; set; } = string.Empty;

    [Key]
    [Required]
    [StringLength(128)]
    public string email { get; set; } = string.Empty;

    [Required]
    public TicketType Ticket { get; set; } = TicketType.Undefined;

    [Required]
    public int Pnumber { get; set; }
}
