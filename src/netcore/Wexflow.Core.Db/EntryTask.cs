namespace Wexflow.Core.Db
{
    public class EntryTask
    {
        public static readonly string DocumentName = "taskentries";

        public int Id { get; set; }
        public int EntryId { get; set; }
        public int TaskId { get; set; }
        public int State { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
