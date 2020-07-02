using CommandLine;
using System.Collections.Generic;

namespace Wexflow.CommandLineParserClient.Options.Workflow
{
    [Verb("create", HelpText = "Creates a new workflow with its tasks and corresponding task settings")]
    public class CreateWorkflowOptions
    {
        [Option('n', "name", Required = true)]
        public string Name { get; set; }

        [Option('d', "description", Required = false, Default = "")]
        public string Description { get; set; }

        [Option('e', "enabled", Required = false, Default = true)]
        public bool Enabled { get; set; }

        [Option('a', "approval", Required = false, Default = false)]
        public bool Approval { get; set; }

        [Option('p', "parallel", Required = false, Default = true)]
        public bool ParallelJobs { get; set; }

        [Option('l', "launch", Required = true)]
        public int LaunchType { get; set; }

        [Option('c', "cron", Required = false, Default = "")]
        public string CronExpression { get; set; }

        [Option('t', "tasks", Required = true, Separator = ';', HelpText = "")]
        public IEnumerable<string> Tasks { get; set; }

        [Option('s', "settings", Required = true, Separator = ';', HelpText = "")]
        public IEnumerable<string> TaskSettings { get; set; }

        [Option("prerequisites", Required = false, Separator = ';', HelpText = "")]
        public IEnumerable<string> TaskPrerequisites { get; set; }
    }
}
