using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;
using System.Linq;

namespace Wexflow.Scripts.LiteDB
{
    class Program
    {
        private static IConfiguration config;

        static void Main()
        {
            try
            {
                config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = config["workflowsFolder"];
                var db = new Db(config["connectionString"]);

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

                BuildDatabase("Windows", "windows");
                BuildDatabase("Linux", "linux");
                BuildDatabase("Mac OS X", "macos");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        private static void BuildDatabase(string info, string platformFolder)
        {
            Console.WriteLine($"=== Build {info} database ===");
            var path1 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow.db");
            var path2 = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Database", "Wexflow-log.db");
            var connString = "Filename=" + path1 + "; Mode=Exclusive";

            var workflowsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "netcore", platformFolder, "Wexflow", "Workflows");

            if (!Directory.Exists(workflowsFolder)) throw new DirectoryNotFoundException("Invalid workflows folder: " + workflowsFolder);
            if (File.Exists(path1)) File.Delete(path1);
            if (File.Exists(path2)) File.Delete(path2);

            var db = new Db(connString);
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
    }
}
