using System;

namespace TuTa.Wms
{
    public static class WmsDomainHelper
    {

        public static string NotWhiteSpaceCheck(string value, string parameterName)
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new Exception($"{parameterName}的值无效");

            return value;
        }

        public static int? NotNegativeOrZeroCheck(int? value, string parameterName)
        {
            if (value != null && value <= 0)
                throw new Exception($"{parameterName}的值无效");

            return value;
        }
    }
}
