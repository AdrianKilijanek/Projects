using AKRek.Localization;
using Volo.Abp.Application.Services;

namespace AKRek;

/* Inherit your application services from this class.
 */
public abstract class AKRekAppService : ApplicationService
{
    protected AKRekAppService()
    {
        LocalizationResource = typeof(AKRekResource);
    }
}
