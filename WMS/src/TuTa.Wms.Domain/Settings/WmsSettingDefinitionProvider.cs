using Volo.Abp.Settings;

namespace TuTa.Wms._Settings;

public class WmsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(WmsSettings.MySetting1));
    }
}
