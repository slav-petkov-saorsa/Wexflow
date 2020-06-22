using System;
using System.Xml.Linq;

namespace Wexflow.Core
{
    public class TaskPrerequisite
    {
        public int TaskId { get; private set; }

        private TaskPrerequisite(int taskId)
        {
            this.TaskId = taskId;
        }

        public static TaskPrerequisite FromElement(XElement element)
        {
            var taskIdAttribute = element.Attribute("taskId");
            if (taskIdAttribute == null)
            {
                throw new Exception("Prerequisite task id not found");
            }
            
            var taskId = int.Parse(taskIdAttribute.Value);

            return new TaskPrerequisite(taskId);
        }
    }
}
