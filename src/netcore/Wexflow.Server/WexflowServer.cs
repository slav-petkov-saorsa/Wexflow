using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Wexflow.Core;

namespace Wexflow.Server
{
    public class WexflowServer
    {
        private static string superAdminUsername;

        public static PollingFileSystemWatcher Watcher;
        public static IConfiguration Config;
        public static WexflowEngine WexflowEngine;

        public static void Main(string[] args)
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);

            var wexflowSettingsFile = Config["WexflowSettingsFile"];
            superAdminUsername = Config["SuperAdminUsername"];
            var enableWorkflowsHotFolder = bool.Parse(Config["EnableWorkflowsHotFolder"]);
            WexflowEngine = new WexflowEngine(wexflowSettingsFile, enableWorkflowsHotFolder, superAdminUsername);

            if (enableWorkflowsHotFolder)
            {
                InitializePollingFileSystemWatcher();
            }
            else
            {
                Logger.Info("Workflows hot folder is disabled.");
            }

            WexflowEngine.Run();

            var port = int.Parse(Config["WexflowServicePort"]);

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel((context, options) =>
                {
                    options.ListenAnyIP(port);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();

            Console.Write("Press any key to stop Wexflow server...");
            Console.ReadKey();
            WexflowEngine.Stop(true, true);
        }

        public static void InitializePollingFileSystemWatcher()
        {
            Logger.Info("Initializing PollingFileSystemWatcher...");
            Watcher = new PollingFileSystemWatcher(WexflowEngine.WorkflowsFolder, "*.xml");

            // Add event handlers.
            Watcher.ChangedDetailed += OnChanged;

            // Begin watching.
            Watcher.Start();
            Logger.InfoFormat("PollingFileSystemWatcher.Path={0}", Watcher.Path);
            Logger.InfoFormat("PollingFileSystemWatcher.Filter={0}", Watcher.Filter);
            Logger.Info("PollingFileSystemWatcher Initialized.");
        }

        private static void OnChanged(object source, PollingFileSystemEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                var path = Path.Combine(change.Directory, change.Name);
                switch (change.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        Logger.Info("PollingFileSystemWatcher.OnCreated");
                        try
                        {
                            var admin = WexflowEngine.GetUser(superAdminUsername);
                            WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while creating the workflow {0}", ex, path);
                        }
                        break;
                    case WatcherChangeTypes.Changed:
                        Logger.Info("PollingFileSystemWatcher.OnChanged");
                        try
                        {
                            var admin = WexflowEngine.GetUser(superAdminUsername);
                            WexflowEngine.SaveWorkflowFromFile(admin.GetDbId(), Core.Db.UserProfile.SuperAdministrator, path, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while updating the workflow {0}", ex, path);
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                        Logger.Info("PollingFileSystemWatcher.OnDeleted");
                        try
                        {
                            var removedWorkflow = WexflowEngine.Workflows.SingleOrDefault(wf => wf.FilePath == path);
                            if (removedWorkflow != null)
                            {
                                WexflowEngine.DeleteWorkflow(removedWorkflow.DbId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Error while deleting the workflow {0}", ex, path);
                        }
                        break;
                }
            }
        }

    }
}
