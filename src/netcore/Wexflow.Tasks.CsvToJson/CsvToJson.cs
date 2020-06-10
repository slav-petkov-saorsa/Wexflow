﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.CsvToJson
{
    public class CsvToJson : Task
    {
        public string Separator { get; private set; }

        public CsvToJson(XElement xe, Workflow wf) : base(xe, wf)
        {
            Separator = GetSetting("separator", ";");
        }

        public override TaskStatus Run()
        {
            Info("Converting CSV files to JSON files...");

            bool success;
            bool atLeastOneSuccess = false;
            try
            {
                success = ConvertFiles(ref atLeastOneSuccess);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting files.", e);
                success = false;
            }

            var status = WorkflowStatus.Success;

            if (!success && atLeastOneSuccess)
            {
                status = WorkflowStatus.Warning;
            }
            else if (!success)
            {
                status = WorkflowStatus.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool ConvertFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            var csvFiles = SelectFiles();

            foreach (var csvFile in csvFiles)
            {
                try
                {
                    var json = Convert(csvFile.Path, Separator);
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(csvFile.FileName) + ".json");
                    File.WriteAllText(destPath, json);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The CSV file {0} has been converted -> {1}", csvFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while converting the CSV file {0}: {1}", csvFile.Path, e.Message);
                    success = false;
                }
            }

            return success;
        }

        private string Convert(string path, string separator)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                csv.Add(line.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
            }

            var properties = lines[0].Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j], csv[i][j]);
                }

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }

    }
}
