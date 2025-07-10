using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKRek.Customers;
using Xunit;

namespace AKRek.EntityFrameworkCore.Applications.Customers;

[Collection(AKRekTestConsts.CollectionDefinitionName)]
internal class EfCoreCustomerAppService_Tests : CustomerAppService_Tests<AKRekEntityFrameworkCoreTestModule>
{
}
