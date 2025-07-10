using AKRek.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AKRek.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AKRekController : AbpControllerBase
{
    protected AKRekController()
    {
        LocalizationResource = typeof(AKRekResource);
    }
}
