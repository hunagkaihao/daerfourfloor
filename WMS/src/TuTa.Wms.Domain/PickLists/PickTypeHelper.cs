using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wms.ConfigTool;

namespace TuTa.Wms.PickLists
{
    public static class PickTypeHelper
    {
        public static string PickTypeToChinese(int pickType)
        {
            PickTypeMap typeMap = Settings.Options.PickTypeMaps.FirstOrDefault(o => o.PickTypeNo == pickType);
            if (typeMap == null)
                throw new Exception($"未知领用类型: {pickType}，若增加了新的领用类型，请配置到配置文件");

            return typeMap.PickTypeName;
        }

        public static int ChineseToPickType(string chinese)
        {
            PickTypeMap typeMap = Settings.Options.PickTypeMaps.FirstOrDefault(o => o.PickTypeName == chinese);
            if (typeMap == null)
                throw new Exception($"未知领用类型: {chinese}，若增加了新的领用类型，请配置到配置文件");

            return typeMap.PickTypeNo;
        }

        public static List<PickTypeMap> GetNoPlanPickTypes()
        {
            return Settings.Options.PickTypeMaps.Where(o => o.IsNoPlanType == true).ToList();
        }
    }
}
