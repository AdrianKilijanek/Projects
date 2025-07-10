using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace AKRek.Customers;

public abstract class CustomerAppService_Tests<TStartupModule> : AKRekApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ICustomerAppService _customerAppService;

    protected CustomerAppService_Tests()
    {
        _customerAppService = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Customers()
    {
        //Act
        var result = await _customerAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "Patryk");
    }

    [Fact]
    public async Task Should_Create_A_Valid_Customer()
    {
        //Act
        var result = await _customerAppService.CreateAsync(
            new CreateUpdateCustomerDto
            {
                Name = "Jan",
                SurName = "Kowalski",
                email = "Kowalski@gmail.com",
                Ticket = TicketType.Normalny,
                Pnumber = 456787957,
            }
        );


        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Jan");
        result.Ticket.ShouldBe(TicketType.Normalny);
    }



    [Fact]
    public async Task Should_Create_A_Valid_Customer_VIP()
    {
        //Act
        var result = await _customerAppService.CreateAsync(
            new CreateUpdateCustomerDto
            {
                Name = "Jan",
                SurName = "Kowalski",
                email = "Kowalski@gmail.com",
                Ticket = TicketType.VIP,
                Pnumber = 456787957,
            }
        );


        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Ticket.ShouldBe(TicketType.VIP);
    }

    [Fact]
    public async Task Should_Create_A_Valid_Customer_Ulgowy()
    {
        //Act
        var result = await _customerAppService.CreateAsync(
            new CreateUpdateCustomerDto
            {
                Name = "Jan",
                SurName = "Kowalski",
                email = "Kowalski@gmail.com",
                Ticket = TicketType.Ulgowy,
                Pnumber = 456787957,
            }
        );


        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Ticket.ShouldBe(TicketType.Ulgowy);
    }


    [Fact]
    public async Task Should_Not_Create_A_Customer_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _customerAppService.CreateAsync(
                new CreateUpdateCustomerDto
                {
                    Name = "",
                    SurName = "Kowalski",
                    email = "Kowalski2@gmail.com",
                    Ticket = TicketType.Normalny,
                    Pnumber = 456787957,
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }

    [Fact]
    public async Task Should_Not_Create_A_Customer_Without_Email()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _customerAppService.CreateAsync(
                new CreateUpdateCustomerDto
                {
                    Name = "Jan",
                    SurName = "Kowalski",
                    email = "",
                    Ticket = TicketType.Normalny,
                    Pnumber = 456787957,
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "email"));
    }


    [Fact]
    public async Task Should_Not_Create_A_Customer_Without_Surname()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _customerAppService.CreateAsync(
                new CreateUpdateCustomerDto
                {
                    Name = "Jan",
                    SurName = "",
                    email = "Kowalski2@gmail.com",
                    Ticket = TicketType.Undefined,
                    Pnumber = 456787957,
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "SurName"));
    }



}
