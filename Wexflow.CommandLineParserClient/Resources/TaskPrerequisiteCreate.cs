using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Wexflow.CommandLineParserClient.Common.Extensions;

namespace Wexflow.CommandLineParserClient.Resources
{
    public class TaskPrerequisiteCreate
    {
        public int TaskId { get; set; }
        [JsonIgnore]
        public int ChildTaskId { get; set; }

        public static IEnumerable<TaskPrerequisiteCreate> FromPrerequisitesString(string prerequisitesString)
        {
            var result = new List<TaskPrerequisiteCreate>();
            
            var prerequisiteProperties = prerequisitesString
                .Split("|")
                .Select(parts => parts.Split("="))
                .ToDictionary(key => key[0], value => value[1]);
            var childTaskId = prerequisiteProperties.ValueOrDefault("TaskId", Convert.ToInt32, 0);
            var dependentTaskIds = prerequisiteProperties.ValueOrDefault("DependsOn", s => s.Split(","), Enumerable.Empty<string>())
                .Select(stringId => Convert.ToInt32(stringId));

            foreach (var taskId in dependentTaskIds)
            {
                result.Add(new TaskPrerequisiteCreate
                {
                    ChildTaskId = childTaskId,
                    TaskId = taskId
                });
            }
            
            return result;
        }
    }
}
