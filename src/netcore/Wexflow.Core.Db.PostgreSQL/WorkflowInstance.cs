using System;

namespace Wexflow.Core.Db.PostgreSQL
{
    public class WorkflowInstance : Workflow
    {
        public Guid InstanceId { get; set; }
    }
}
