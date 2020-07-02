using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Wexflow.CommandLineParserClient.Options.Workflow;

namespace Wexflow.CommandLineParserClient.Resources
{
    public class WorkflowCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonPropertyName("IsEnabled")]
        public bool Enabled { get; set; }
        public bool IsApproval { get; set; }
        public bool EnableParallelJobs { get; set; }
        public int LaunchType { get; set; }
        public string CronExpression { get; set; }

        [JsonIgnore]
        public IEnumerable<TaskCreate> Tasks { get; set; } = new List<TaskCreate>();

        public static WorkflowCreate FromCommandLineOptions(CreateWorkflowOptions options)
        {
            var resource = new WorkflowCreate
            {
                Name = options.Name,
                Description = options.Description,
                Enabled = options.Enabled,
                LaunchType = options.LaunchType,
                CronExpression = options.CronExpression,
                IsApproval = options.Approval,
                EnableParallelJobs = options.ParallelJobs
            };

            var settings = options.TaskSettings
                .Select(SettingCreate.FromSettingString)
                .ToList();
            var tasks = options.Tasks
                .Select(task => TaskCreate.FromStringTaskWithSettings(task, settings, options.TaskPrerequisites))
                .ToList();
            resource.Tasks = tasks;

            return resource;
        }
    }
}