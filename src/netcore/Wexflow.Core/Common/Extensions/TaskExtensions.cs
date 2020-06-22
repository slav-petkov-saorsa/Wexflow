using System.Linq;

namespace Wexflow.Core.Common.Extensions
{
    public static class TaskExtensions
    {
        public static bool HasUnsatisfiedPrecondition(this Task task)
        {
            var prerequisiteTasksIds = task.Prerequisites.Select(prerequisite => prerequisite.TaskId);

            return task.Workflow.Tasks
                .Where(task => prerequisiteTasksIds.Contains(task.Id))
                .Any(task => task.State != TaskState.Completed);
        }
    }
}
