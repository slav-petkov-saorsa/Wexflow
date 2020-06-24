using System;

namespace Wexflow.Server.Contracts.Workflow
{
    public class ActiveWorkflowInstance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid InstanceId { get; set; }
    }
}