using CommandLine;

namespace Wexflow.CommandLineParserClient.Options.Workflow
{
    [Verb("start", HelpText = "Starts a workflow by id")]
    public class StartWorfklowOptions
    {
        [Option('w', "workflow", Required = true)]
        public int WorkflowId { get; set; }
    }
}
