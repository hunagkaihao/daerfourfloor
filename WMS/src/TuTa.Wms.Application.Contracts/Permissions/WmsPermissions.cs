namespace TuTa.Wms.Permissions;

public static class WmsPermissions
{
    public const string GroupName = "Wms";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public const string ReadPermission = GroupName + ".Read";
    public const string DeletePermission = GroupName + ".Delete";
    public const string EditPermission = GroupName + ".Edit";
    public const string AddPermission = GroupName + ".Add";
}
