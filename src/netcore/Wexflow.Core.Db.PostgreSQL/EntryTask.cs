namespace Wexflow.Core.Db.PostgreSQL
{
    public class EntryTask : Core.Db.EntryTask
    {
        public static readonly string ColumnName_Id = "Id";
        public static readonly string ColumnName_EntryId = "EntryId";
        public static readonly string ColumnName_TaskId = "TaskId";
        public static readonly string ColumnName_State = "State";

        public static readonly string TableStruct = $"(" +
            $"{ColumnName_Id} serial PRIMARY KEY," +
            $"{ColumnName_EntryId} integer NOT NULL," +
            $"{ColumnName_TaskId} integer NOT NULL," +
            $"{ColumnName_State} integer NOT NULL," +
            $"FOREIGN KEY ({ColumnName_EntryId}) REFERENCES {Core.Db.Entry.DocumentName} ({Entry.ColumnName_Id})" +
            $")";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
