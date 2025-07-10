using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace AKRek.Customers;

public class CustomerDto:AuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public TicketType Ticket { get; set; }
    public int Pnumber { get; set; }
}
