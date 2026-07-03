using System.Text.RegularExpressions;
using TuTa.Wms.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace TuTa.Wms.Permissions;

public class WmsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        //var myGroup = context.AddGroup(WmsPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(WmsPermissions.MyPermission1, L("Permission:MyPermission1"));

        var wmsGroup = context.AddGroup(WmsPermissions.GroupName);
        wmsGroup.AddPermission(WmsPermissions.AddPermission, L("Permission:AddPermission"));
        wmsGroup.AddPermission(WmsPermissions.DeletePermission, L("Permission:DeletePermission"));
        wmsGroup.AddPermission(WmsPermissions.EditPermission, L("Permission:EditPermission"));
        wmsGroup.AddPermission(WmsPermissions.ReadPermission, L("Permission:ReadPermission"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WmsResource>(name);
    }
}
