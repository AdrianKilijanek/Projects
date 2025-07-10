using AKRek.Customers;
using AutoMapper;

namespace AKRek;

public class AKRekApplicationAutoMapperProfile : Profile
{
    public AKRekApplicationAutoMapperProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateUpdateCustomerDto, Customer>();
    }
}
