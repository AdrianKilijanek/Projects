using AKRek.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace AKRek;

public class AKRekDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Customer, Guid> _customerRepository;

    public AKRekDataSeederContributor(IRepository<Customer, Guid> customerRepository)
    {
        _customerRepository = customerRepository;
    }
    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _customerRepository.GetCountAsync() <= 0)
        {
            await _customerRepository.InsertAsync(
                new Customer
                {
                    Name = "John",
                    Surname = "Smith",
                    Email = "JohnSmith12@gmail.com",
                    Ticket = TicketType.Normalny,
                    Pnumber = 734853903
                },
                autoSave: true
            );

            await _customerRepository.InsertAsync(
                new Customer
                {
                    Name = "Brunhilda",
                    Surname = "Maciejczyk",
                    Email = "bmaciej@wp.pl",
                    Ticket = TicketType.VIP,
                    Pnumber = 504837932
                },
                autoSave: true
            );

            await _customerRepository.InsertAsync(
                new Customer
                {
                    Name = "Patryk",
                    Surname = "Kaczkowski",
                    Email = "pakczkowski9@gmail.com",
                    Ticket = TicketType.Ulgowy,
                    Pnumber = 684302645
                },
                autoSave: true
            );
        }
    }
}
