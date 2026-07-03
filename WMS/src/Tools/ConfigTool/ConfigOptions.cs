using System.Collections.Generic;

namespace Wms.ConfigTool
{
    public class PlcHeartBeatSet
    {
        public string PlcName { get; set; } = string.Empty;
        public string HeartTagName { get; set; } = string.Empty; //需要为整型数据
        public int CycleTime { get; set; }
    }

    public class MjjAvoidPos
    {
        public string LmTarget { get; set; } = string.Empty;
        public byte MjjAvoidCol { get; set; } = 0;
        public byte MjjAvoidZY { get; set; } = 0;
    }

    public class PickTypeMap 
    {
        public int PickTypeNo { get; set; }

        public string PickTypeName { get; set; }

        public bool IsNoPlanType { get; set; }
    }


    public class ConfigOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string SqliteLogConnString { get; set; } = "";
        public int LogClearInterval { get; set; } = 0;
        public int LogMaxVolume { get; set; } = 0;
        public string RedisConnStr { get; set; } = "";
        public int DefaultRedisNo { get; set; } = 0;
        public int PlcRedisNo { get; set; } = 0;
        public List<PlcHeartBeatSet> HeartBeatsFromPlc { get; set; } = new List<PlcHeartBeatSet>();
        public List<PlcHeartBeatSet> HeartBeatsToPlc { get; set; } = new List<PlcHeartBeatSet>();
        public List<string> PlcTagMonitors { get; set; } = new List<string>();

        public bool RemovePlcTagTempValueOnStart { get; set; }

        public List<PickTypeMap> PickTypeMaps { get; set; }

    }

    public class Jwt
    {
        public string Audience { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;
        public int ExpirationTime { get; set; } = 0;
    }
}
