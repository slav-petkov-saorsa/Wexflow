using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Wexflow.Core;
using Wexflow.Core.Db;

namespace Wexflow.Server
{
    public partial class WexflowServer : ServiceBase
    {
        public static NameValueCollection Config = ConfigurationManager.AppSettings;

        private static string settingsFile = Config["WexflowSettingsFile"];
        private static string superAdminUsername = Config["SuperAdminUsername"];
        private static bool enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);
        private static bool enableRecordsHotFolder = bool.Parse(Config["EnableRecordsHotFolder"]);
        private static bool enableEmailNotifications = bool.Parse(Config["EnableEmailNotifications"]);
        private static string smtpHost = Config["Smtp.Host"];
        private static int smtpPort = int.Parse(Config["Smtp.Port"]);
        private static bool smtpEnableSsl = bool.Parse(Config["Smtp.EnableSsl"]);
        private static string smtpUser = Config["Smtp.User"];
        private static string smtpPassword = Config["Smtp.Password"];
        private static string smtpFrom = Config["Smtp.From"];

        public static FileSystemWatcher WorkflowsWatcher;
        public static FileSystemWatcher RecordsWatcher;
        public static WexflowEngine WexflowEngine = new WexflowEngine(settingsFile
            , enableWorkflowsHotFolder
            , superAdminUsername
            , enableEmailNotifications
            , smtpHost
            , smtpPort
            , smtpEnableSsl
            , smtpUser
            , smtpPassword
            , smtpFrom
            );

        private IDisposable _webApp;

        public WexflowServer()
        {
            InitializeComponent();
            ServiceName = "Wexflow";
            Thread startThread = new Thread(StartThread) { IsBackground = true };
            startThread.Start();
        }

        private void StartThread()
        {
            if (enableWorkflowsHotFolder)
            {
                InitializeWorkflowsFileSystemWatcher();
            }
            else
            {
                Logger.Info("Workflows hot folder is disabled.");
            }

            if (enableRecordsHotFolder)
            {
                // On file found.
                foreach (var file in Directory.GetFiles(WexflowEngine.RecordsHotFolder))
                {
                    SaveRecord(file);
                }

                // On file created.
                InitializeRecordsFileSystemWatcher();
            }
            else
            {
                Logger.Info("Records hot folder is disabled.");
            }

            WexflowEngine.Run();
        }

        protected override void OnStart(string[] args)
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
            }

            var port = int.Parse(Config["WexflowServicePort"]);
            var url = "http://+:" + port;
            _webApp = WebApp.Start<Startup>(url);
        }

        protected override void OnStop()
        {
            WexflowEngine.Stop(true, true);

            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
        }

        public static void InitializeWorkflowsFileSystemWatcher()
        {
            Logger.Info("Initializing workflows FileSystemWatcher...");
            WorkflowsWatcher = new FileSystemWatcher
            {
                Path = WexflowEngine.WorkflowsFolder,
                Filter = "*.xml",
                IncludeSubdirectories = false
            };

            // Add event handlers.
            WorkflowsWatcher.Created += OnWorkflowCreated;
            WorkflowsWatcher.Changed += OnWorkflowChanged;
            WorkflowsWatcher.Deleted += OnWorkflowDeleted;

            // Begin watching.
            WorkflowsWatcher.EnableRaisingEvents = true;
            Logger.InfoFormat("Workflow.FileSystemWatcher.Path={0}", WorkflowsWatcher.Path);
            Logger.InfoFormat("Workflow.FileSystemWatcher.Filter={0}", WorkflowsWatcher.Filter);
            Logger.InfoFormat("Workflow.FileSystemWatcher.EnableRaisingEvents={0}", WorkflowsWatcher.EnableRaisingEvents);
            Logger.Info("Workflows FileSystemWatcher Initialized.");
        }

        private static void OnWorkflowCreated(object source, FileSystemEventArgs e)
        {
            if (!IsDirectory(e.FullPath))
            {
                Logger.Info("Workflow.FileSystemWatcher.OnCreated");
                try
                {
                    var admin = WexflowEngine.GetUser(superAdminUsername);
                    WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, e.FullPath, true);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error while creating the workflow {0}", ex, e.FullPath);
                }
            }
        }

        private static void OnWorkflowChanged(object source, FileSystemEventArgs e)
        {
            if (!IsDirectory(e.FullPath))
            {
                Logger.Info("Workflow.FileSystemWatcher.OnChanged");
                try
                {
                    Thread.Sleep(500);
                    var admin = WexflowEngine.GetUser(superAdminUsername);
                    WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, e.FullPath, true);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error while updating the workflow {0}", ex, e.FullPath);
                }
            }
        }

        private static void OnWorkflowDeleted(object source, FileSystemEventArgs e)
        {
            Logger.Info("Workflow.FileSystemWatcher.OnDeleted");
            try
            {
                var removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.FilePath == e.FullPath);
                if (removedWorkflow != null)
                {
                    WexflowEngine.DeleteWorkflow(removedWorkflow.DbId);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while deleting the workflow {0}", ex, e.FullPath);
            }
        }

        public static void InitializeRecordsFileSystemWatcher()
        {
            Logger.Info("Initializing records FileSystemWatcher...");
            RecordsWatcher = new FileSystemWatcher
            {
                Path = WexflowEngine.RecordsHotFolder,
                Filter = "*.*",
                IncludeSubdirectories = false
            };

            // Add event handlers.
            RecordsWatcher.Created += OnRecordCreated;

            // Begin watching.
            RecordsWatcher.EnableRaisingEvents = true;
            Logger.InfoFormat("Record.FileSystemWatcher.Path={0}", RecordsWatcher.Path);
            Logger.InfoFormat("Record.FileSystemWatcher.Filter={0}", RecordsWatcher.Filter);
            Logger.InfoFormat("Record.FileSystemWatcher.EnableRaisingEvents={0}", RecordsWatcher.EnableRaisingEvents);
            Logger.Info("Records FileSystemWatcher Initialized.");
        }

        private static void OnRecordCreated(object source, FileSystemEventArgs e)
        {
            if (!IsDirectory(e.FullPath))
            {
                Logger.Info("Record.FileSystemWatcher.OnCreated");
                try
                {
                    Thread.Sleep(1000);
                    SaveRecord(e.FullPath);
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
                {
                    Logger.InfoFormat("There is a sharing violation for the file {0}.", e.FullPath);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error while creating the record {0}", ex, e.FullPath);
                }
            }
        }

        private static void SaveRecord(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var destDir = Path.Combine(WexflowEngine.RecordsTempFolder, WexflowEngine.DbFolderName, "-1", Guid.NewGuid().ToString());
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            var destPath = Path.Combine(destDir, fileName);
            File.Move(filePath, destPath);
            var parentDir = Path.GetDirectoryName(destPath);
            if (WexflowEngine.IsDirectoryEmpty(parentDir))
            {
                Directory.Delete(parentDir);
                var recordTempDir = Directory.GetParent(parentDir).FullName;
                if (WexflowEngine.IsDirectoryEmpty(recordTempDir))
                {
                    Directory.Delete(recordTempDir);
                }
            }

            var admin = WexflowEngine.GetUser(superAdminUsername);
            var record = new Record
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                CreatedBy = admin.GetDbId()
            };

            var version = new Core.Db.Version
            {
                FilePath = destPath
            };

            List<Core.Db.Version> versions = new List<Core.Db.Version>() { version };

            var recordId = WexflowEngine.SaveRecord("-1", record, versions);
            if (recordId != "-1")
            {
                Logger.Info($"Record inserted from file {filePath}. RecordId: {recordId}");
            }
            else
            {
                Logger.Error($"An error occured while inserting a record from the file {filePath}.");
            }
        }

        private static bool IsDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            var isDir = attr.HasFlag(FileAttributes.Directory);

            return isDir;
        }

    }
}
