using Microsoft.Extensions.Localization;
using AKRek.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AKRek;

[Dependency(ReplaceServices = true)]
public class AKRekBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AKRekResource> _localizer;

    public AKRekBrandingProvider(IStringLocalizer<AKRekResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
