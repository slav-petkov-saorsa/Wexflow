using CommandLine;

namespace Wexflow.CommandLineParserClient.Options.Workflow
{
    [Verb("list", HelpText = "List workflows in the system")]
    public class ListWorkflowsOptions
    {
        [Option('a', "active", Required = false, Default = true)]
        public bool Active { get; set; }

        [Option('d', "daily", Required = false, Default = false)]
        public bool Daily { get; set; }
    }
}
