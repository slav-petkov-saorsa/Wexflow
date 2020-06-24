using System;

namespace Wexflow.Core.Db
{
    public class Workflow
    {
        public static readonly string DocumentName = "workflows";

        public int Id { get; set; }
        public string Xml { get; set; }

        public string GetDbId()
        {
            return this.Id.ToString();
        }
    }
}
