namespace Wexflow.Core
{
    /// <summary>
    /// Status.
    /// </summary>
    public enum WorkflowStatus
    {
        Undefined = -1,
        /// <summary>
        /// Success.
        /// </summary>
        Success,
        /// <summary>
        /// Warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Error.
        /// </summary>
        Error,
        /// <summary>
        /// Rejected.
        /// </summary>
        Rejected
    }
}