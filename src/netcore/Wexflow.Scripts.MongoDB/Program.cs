using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Wexflow.Core.Db.MongoDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.MongoDB
{
    class Program
    {
        static void Main()
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = config["workflowsFolder"];
                Db db = new Db(config["connectionString"]);
                Helper.InsertWorkflowsAndUser(db, workflowsFolder);

                var records = db.GetRecords(string.Empty);
                if (records.Count() == 0)
                {
                    // Insert document
                    Helper.InsertRecord(db
                        , config["recordsFolder"]
                        , config["documentFile"]
                        , "Document"
                        , "Time card"
                        , "This document needs to be completed."
                        , "Please fill the document."
                        , true
                        , "litedb");

                    // Insert invoice
                    Helper.InsertRecord(db
                        , config["recordsFolder"]
                        , config["invoiceFile"]
                        , "Invoice"
                        , "Invoice Payments Report by Agency - July 2013 to June 2014"
                        , "This document needs to be reviewed."
                        , "Please complete the document."
                        , true
                        , "litedb");

                    // Insert timesheet
                    Helper.InsertRecord(db
                        , config["recordsFolder"]
                        , config["timesheetFile"]
                        , "Timesheet"
                        , "Time Sheet"
                        , "This document needs to be completed."
                        , "Please fill the document."
                        , true
                        , "litedb");

                    // Insert vacation request
                    Helper.InsertRecord(db
                        , config["recordsFolder"]
                        , string.Empty
                        , "Vacations"
                        , "Vacations request"
                        , string.Empty
                        , string.Empty
                        , false
                        , "litedb");
                }

                db.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
