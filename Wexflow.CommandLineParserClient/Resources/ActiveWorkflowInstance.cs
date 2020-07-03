using System;
using System.Collections.Generic;
using System.Text;

namespace Wexflow.CommandLineParserClient.Resources
{
    public class ActiveWorkflowInstance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid InstanceId { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}   Name: {Name}    Instance Id: {InstanceId}";
        }
    }
}
