using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Wexflow.Core.Db.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Helper.InsertWorkflowsAndUser(db);

                var records = db.GetRecords(string.Empty);
                if (records.Count() == 0)
                {
                    // Insert document
                    Helper.InsertRecord(db
                        , "Document"
                        , "Time card"
                        , "This document needs to be completed."
                        , "Please fill the document."
                        , true
                        , "documentFile"
                        , "litedb");

                    // Insert invoice
                    Helper.InsertRecord(db
                        , "Invoice"
                        , "Invoice Payments Report by Agency - July 2013 to June 2014"
                        , "This document needs to be reviewed."
                        , "Please complete the document."
                        , true
                        , "invoiceFile"
                        , "litedb");

                    // Insert timesheet
                    Helper.InsertRecord(db
                        , "Timesheet"
                        , "Time Sheet"
                        , "This document needs to be completed."
                        , "Please fill the document."
                        , true
                        , "timesheetFile"
                        , "litedb");

                    // Insert vacation request
                    Helper.InsertRecord(db
                        , "Vacations"
                        , "Vacations request"
                        , string.Empty
                        , string.Empty
                        , false
                        , string.Empty
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
