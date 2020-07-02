using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.CommandLineParserClient.Common.Extensions;

namespace Wexflow.CommandLineParserClient.Resources
{
    public class TaskCreate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool IsEnabled { get; set; }
        public IEnumerable<TaskPrerequisiteCreate> Prerequisites { get; set; }

        public IEnumerable<SettingCreate> Settings { get; set; } = Enumerable.Empty<SettingCreate>();

        public static TaskCreate FromStringTaskWithSettings(string taskString, IEnumerable<SettingCreate> settings, IEnumerable<string> prerequisites)
        {
            var taskPrerequisites = prerequisites
                .SelectMany(TaskPrerequisiteCreate.FromPrerequisitesString)
                .ToList();
            
            var taskProperties = taskString.Split(":")
                .Select(element => element.Split("="))
                .ToDictionary(key => key[0], value => value[1]);
            var task = new TaskCreate
            {
                Id = taskProperties.ValueOrDefault("Id", Convert.ToInt32, 0),
                Description = taskProperties.ValueOrDefault("Description", Convert.ToString, string.Empty),
                Name = taskProperties.ValueOrDefault("Name", Convert.ToString, string.Empty),
                Type = taskProperties.ValueOrDefault("Type", Convert.ToString, string.Empty),
                IsEnabled = taskProperties.ValueOrDefault("Enabled", Convert.ToBoolean, true)
            };
            task.Settings = settings.Where(s => task.Id == s.TaskId);
            task.Prerequisites = taskPrerequisites.Where(prerequisite => prerequisite.ChildTaskId == task.Id);

            return task;
        }
    }
}
