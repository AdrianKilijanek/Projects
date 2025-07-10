using Volo.Abp.Settings;

namespace AKRek.Settings;

public class AKRekSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AKRekSettings.MySetting1));
    }
}
