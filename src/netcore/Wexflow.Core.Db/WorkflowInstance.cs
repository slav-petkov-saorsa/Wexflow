using System;

namespace Wexflow.Core.Db
{
    public sealed class WorkflowInstance : Workflow
    {
        public Guid InstanceId { get; set; }
    }
}
