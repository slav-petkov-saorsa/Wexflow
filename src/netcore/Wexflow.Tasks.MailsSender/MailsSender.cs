﻿using System;
using System.Collections.Generic;
using System.Linq;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace Wexflow.Tasks.MailsSender
{
    public class MailsSender:Task
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string User { get; }
        public string Password { get; }
        public bool IsBodyHtml { get; }

        public MailsSender(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port"));
            EnableSsl = bool.Parse(GetSetting("enableSsl"));
            User = GetSetting("user");
            Password = GetSetting("password");
            IsBodyHtml = bool.Parse(GetSetting("isBodyHtml", "true"));
        }

        public override TaskStatus Run()
        {
            Info("Sending mails...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                FileInf[] attachments = SelectAttachments();

                foreach (FileInf mailFile in SelectFiles())
                {
                    var xdoc = XDocument.Load(mailFile.Path);
                    var xMails = xdoc.XPathSelectElements("Mails/Mail");

                    int count = 1;
                    foreach (XElement xMail in xMails)
                    {
                        Mail mail;
                        try
                        {
                            mail = Mail.Parse(xMail, attachments);
                            mail.Subject = ParseVariables(mail.Subject);
                            mail.Body = ParseVariables(mail.Body);
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
							ErrorFormat("An error occured while parsing the mail {0}. Please check the XML configuration according to the documentation. Error: {1}", count, e.Message);
                            success = false;
                            count++;
                            continue;
                        }

                        try
                        {
                            mail.Send(Host, Port, EnableSsl, User, Password, IsBodyHtml);
                            InfoFormat("Mail {0} sent.", count);
                            count++;
                            
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            ErrorFormat("An error occured while sending the mail {0}. Error message: {1}", count, e.Message);
                            success = false;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending mails.", e);
                success = false;
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

        private string ParseVariables(string src)
        {
            //
            // Parse local variables.
            //
            var res = string.Empty;
            using (StringReader sr = new StringReader(src))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string pattern = @"{.*?}";
                    Match m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        if (m.Value.StartsWith("{date:"))
                        {
                            var replaceValue = DateTime.Now.ToString(m.Value.Remove(m.Value.Length - 1).Remove(0, 6));
                            line = Regex.Replace(line, pattern, replaceValue);
                        }
                    }
                    foreach (var variable in Workflow.LocalVariables)
                    {
                        line = line.Replace("$" + variable.Key, variable.Value);
                    }
                    sw.WriteLine(line);
                }
                res = sw.ToString();
            }

            //
            // Parse Rest variables.
            //
            var res2 = string.Empty;
            using (StringReader sr = new StringReader(res))
            using (StringWriter sw = new StringWriter())
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var variable in Workflow.RestVariables)
                    {
                        if (variable != null)
                        {
                            line = line.Replace("$" + variable.Key, variable.Value);
                        }
                    }
                    sw.WriteLine(line);
                }
                res2 = sw.ToString();
            }

            return res2.Trim('\r', '\n');
        }

        public FileInf[] SelectAttachments()
        {
            var files = new List<FileInf>();
            foreach (var xSelectFile in GetXSettings("selectAttachments"))
            {
                var xTaskId = xSelectFile.Attribute("value");
                if (xTaskId != null)
                {
                    var taskId = int.Parse(xTaskId.Value);

                    var qf = QueryFiles(Workflow.FilesPerTask[taskId], xSelectFile).ToArray();

                    files.AddRange(qf);
                }
                else
                {
                    var qf = (from lf in Workflow.FilesPerTask.Values
                        from f in QueryFiles(lf, xSelectFile)
                        select f).Distinct().ToArray();

                    files.AddRange(qf);
                }
            }
            return files.ToArray();
        }
    }
}
