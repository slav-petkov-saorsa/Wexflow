using Microsoft.Owin.Hosting;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Wexflow.Core;

namespace Wexflow.Server
{
    public partial class WexflowServer : ServiceBase
    {
        public static NameValueCollection Config = ConfigurationManager.AppSettings;

        private static string settingsFile = Config["WexflowSettingsFile"];
        private static string superAdminUsername = Config["SuperAdminUsername"];
        private static bool enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);

        public static FileSystemWatcher Watcher;
        public static WexflowEngine WexflowEngine = new WexflowEngine(settingsFile, enableWorkflowsHotFolder, superAdminUsername);

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
                InitializeFileSystemWatcher();
            }
            else
            {
                Logger.Info("Workflows hot folder is disabled.");
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

        public static void InitializeFileSystemWatcher()
        {
            Logger.Info("Initializing FileSystemWatcher...");
            Watcher = new FileSystemWatcher
            {
                Path = WexflowEngine.WorkflowsFolder,
                Filter = "*.xml",
                IncludeSubdirectories = false
            };

            // Add event handlers.
            Watcher.Created += OnCreated;
            Watcher.Changed += OnChanged;
            Watcher.Deleted += OnDeleted;

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
            Logger.InfoFormat("FileSystemWatcher.Path={0}", Watcher.Path);
            Logger.InfoFormat("FileSystemWatcher.Filter={0}", Watcher.Filter);
            Logger.InfoFormat("FileSystemWatcher.EnableRaisingEvents={0}", Watcher.EnableRaisingEvents);
            Logger.Info("FileSystemWatcher Initialized.");
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnCreated");
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

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnChanged");
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

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Logger.Info("FileSystemWatcher.OnDeleted");
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

    }
}
