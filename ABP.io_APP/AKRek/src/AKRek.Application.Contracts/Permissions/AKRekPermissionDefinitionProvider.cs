using AKRek.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace AKRek.Permissions;

public class AKRekPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AKRekPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(AKRekPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AKRekResource>(name);
    }
}
