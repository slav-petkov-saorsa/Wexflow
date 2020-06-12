namespace Wexflow.Core
{
    /// <summary>
    /// Task status.
    /// </summary>
    public class TaskStatus
    {
        public static readonly TaskStatus NotDefined = new TaskStatus(WorkflowStatus.Undefined, false);
        public static readonly TaskStatus Completed = new TaskStatus(TaskState.Completed);
        public static readonly TaskStatus Failed = new TaskStatus(TaskState.Failed);

        /// <summary>
        /// State of the task 
        /// </summary>
        public TaskState State { get; set; }
        /// <summary>
        /// Status.
        /// </summary>
        public WorkflowStatus Status { get; set; }
        /// <summary>
        /// If and While condition.
        /// </summary>
        public bool Condition { get; set; }
        /// <summary>
        /// Switch/Case value.
        /// </summary>
        public string SwitchValue { get; set; }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for sequential tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        public TaskStatus(WorkflowStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for If/While flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="condition">Condition value.</param>
        public TaskStatus(WorkflowStatus status, bool condition) : this(status)
        {
            Condition = condition;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for Switch flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="switchValue">Switch value.</param>
        public TaskStatus(WorkflowStatus status, string switchValue) : this(status)
        {
            SwitchValue = switchValue;
        }

        /// <summary>
        /// Creates a new TaskStatus. This constructor is designed for If/While and Switch flowchart tasks.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="condition">Condition value.</param>
        /// <param name="switchValue">Switch value.</param>
        public TaskStatus(WorkflowStatus status, bool condition, string switchValue) : this(status)
        {
            Condition = condition;
            SwitchValue = switchValue;
        }

        private TaskStatus(TaskState state)
        {
            this.State = state;
        }
    }
}