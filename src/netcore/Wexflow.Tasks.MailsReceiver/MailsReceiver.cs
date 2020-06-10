﻿using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.MailsReceiver
{
    public enum Protocol
    {
        Pop3,
        Imap
    }

    public class MailsReceiver : Task
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public int MessageCount { get; private set; }
        public bool DeleteMessages { get; private set; }
        public Protocol Protocol { get; private set; }

        public MailsReceiver(XElement xe, Workflow wf) : base(xe, wf)
        {
            Host = GetSetting("host");
            Port = int.Parse(GetSetting("port", "993"));
            EnableSsl = bool.Parse(GetSetting("enableSsl", "true"));
            User = GetSetting("user");
            Password = GetSetting("password");
            MessageCount = int.Parse(GetSetting("messageCount"));
            DeleteMessages = bool.Parse(GetSetting("deleteMessages", "false"));
            Protocol = (Protocol)Enum.Parse(typeof(Protocol), GetSetting("protocol", "pop3"), true);
        }

        public override TaskStatus Run()
        {
            Info("Receiving mails...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                switch (Protocol)
                {
                    case Protocol.Imap:
                        using (var client = new ImapClient())
                        {
                            client.Connect(Host, Port, EnableSsl);
                            client.Authenticate(User, Password);
                            client.Inbox.Open(FolderAccess.ReadOnly);

                            var uids = client.Inbox.Search(SearchQuery.All);

                            var count = uids.Count();

                            for (int i = Math.Min(MessageCount, count); i > 0; i--)
                            {
                                var message = client.Inbox.GetMessage(uids[i]);
                                string messageFileName = "message_" + i + "_" + string.Format("{0:yyyy-MM-dd-HH-mm-ss-fff}", message.Date);
                                string messagePath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + ".eml");
                                message.WriteTo(messagePath);
                                Files.Add(new FileInf(messagePath, Id));
                                InfoFormat("Message {0} received. Path: {1}", i, messagePath);

                                // save attachments
                                var j = 0;
                                var attachments = message.Attachments.ToList();
                                foreach (var attachment in attachments)
                                {
                                    if (attachment.IsAttachment)
                                    {
                                        string attachmentPath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + "_" + (attachment is MessagePart ? ++j + ".eml" : ((MimePart)attachment).FileName));

                                        if (attachment is MessagePart)
                                        {
                                            ((MessagePart)attachment).WriteTo(attachmentPath);
                                        }
                                        else
                                        {
                                            using (var stream = File.Create(attachmentPath))
                                            {
                                                ((MimePart)attachment).Content.DecodeTo(stream);
                                            }
                                        }

                                        Files.Add(new FileInf(attachmentPath, Id));
                                        InfoFormat("Attachment {0} of mail {1} received. Path: {2}", (attachment is MessagePart ? j + ".eml" : ((MimePart)attachment).FileName), i, attachmentPath);
                                    }
                                }


                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }

                            client.Disconnect(true);
                        }
                        break;
                    case Protocol.Pop3:
                        using (var client = new Pop3Client())
                        {
                            client.Connect(Host, Port, EnableSsl);
                            client.AuthenticationMechanisms.Remove("XOAUTH2");
                            client.Authenticate(User, Password);

                            var count = client.GetMessageCount();

                            // We want to download messages
                            // Messages are numbered in the interval: [1, messageCount]
                            // Ergo: message numbers are 1-based.
                            // Most servers give the latest message the highest number
                            for (int i = Math.Min(MessageCount, count); i > 0; i--)
                            {
                                var message = client.GetMessage(i);
                                string messageFileName = "message_" + i + "_" + string.Format("{0:yyyy-MM-dd-HH-mm-ss-fff}", message.Date);
                                string messagePath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + ".eml");
                                message.WriteTo(messagePath);
                                Files.Add(new FileInf(messagePath, Id));
                                InfoFormat("Message {0} received. Path: {1}", i, messagePath);

                                // save attachments
                                var attachments = message.Attachments.ToList();
                                foreach (var attachment in attachments)
                                {
                                    if (attachment.IsAttachment)
                                    {
                                        string attachmentPath = Path.Combine(Workflow.WorkflowTempFolder, messageFileName + "_" + attachment.ContentId);
                                        attachment.WriteTo(attachmentPath);
                                        Files.Add(new FileInf(attachmentPath, Id));
                                        InfoFormat("Attachment {0} of mail {1} received. Path: {2}", attachment.ContentId, i, attachmentPath);
                                    }
                                }

                                if (DeleteMessages)
                                {
                                    client.DeleteMessage(i);
                                }

                                if (!atLeastOneSucceed)
                                {
                                    atLeastOneSucceed = true;
                                }
                            }

                            client.Disconnect(true);
                        }
                        break;
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while receiving mails.", e);
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
    }
}
