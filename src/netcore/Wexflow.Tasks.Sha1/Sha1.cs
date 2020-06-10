﻿using System;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Wexflow.Tasks.Sha1
{
    public class Sha1 : Task
    {
        public Sha1(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating SHA-1 hashes...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("SHA1_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        var sha1 = GetSha1(file.Path);
                        if (xdoc.Root != null)
                        {
                            xdoc.Root.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("sha1", sha1)));
                        }
                        InfoFormat("SHA-1 hash of the file {0} is {1}", file.Path, sha1);
                        
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the SHA-1 hash of the file {0}", e, file.Path);
                        success = false;
                    }
                }
                xdoc.Save(md5Path);
                Files.Add(new FileInf(md5Path, Id));
            }

            var status = WorkflowStatus.Success;

            if (!success && atLeastOneSucceed)
            {
                status = WorkflowStatus.Warning;
            }
            else if (!success)
            {
                status = WorkflowStatus.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private string GetSha1(string filePath)
        {
            var sb = new StringBuilder();
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    var bytes = sha1.ComputeHash(stream);

                    foreach (byte bt in bytes)
                    {
                        sb.Append(bt.ToString("x2"));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
