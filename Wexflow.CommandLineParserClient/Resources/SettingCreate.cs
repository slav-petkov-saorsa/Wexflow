using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Wexflow.CommandLineParserClient.Resources
{
    public class SettingCreate
    {
        [JsonIgnore]
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public static SettingCreate FromSettingString(string settingString)
        {
            var settingProperties = settingString
                .Split("|")
                .Select(parts => parts.Split("="))
                .ToDictionary(key => key[0], value => value[1]);

            var setting = new SettingCreate
            {
                TaskId = Convert.ToInt32(settingProperties["TaskId"]),
                Name = settingProperties["Name"],
                Value = settingProperties["Value"]
            };
            
            return setting;
        }
    }
}
