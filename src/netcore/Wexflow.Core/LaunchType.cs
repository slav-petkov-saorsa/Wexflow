namespace Wexflow.Core
{
    /// <summary>
    /// Launch type.
    /// </summary>
    public enum LaunchType
    {
        /// <summary>
        /// The workflow must be triggered manually to start.
        /// </summary>
        Trigger,
        /// <summary>
        /// The workflow starts depending on the cron scheduling expression
        /// </summary>
        Cron
    }
}