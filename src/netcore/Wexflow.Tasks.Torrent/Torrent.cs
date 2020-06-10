﻿using MonoTorrent.Client;
using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Torrent
{
    public class Torrent : Task
    {
        public string SaveFolder { get; set; }

        public Torrent(XElement xe, Workflow wf) : base(xe, wf)
        {
            SaveFolder = GetSetting("saveFolder");
        }

        public override TaskStatus Run()
        {
            Info("Downloading torrents...");

            bool success;
            var atLeastOneSuccess = false;
            try
            {
                success = Download(ref atLeastOneSuccess);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while creating tgz.", e);
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

        private bool Download(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var torrents = SelectFiles();
                foreach (var torrent in torrents)
                {
                    success &= DownloadTorrent(torrent.Path);
                    if (!atLeastOneSuccess && success) atLeastOneSuccess = true;
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading torrents: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool DownloadTorrent(string path)
        {
            try
            {
                ClientEngine engine = new ClientEngine(new EngineSettings());

                MonoTorrent.Torrent torrent = MonoTorrent.Torrent.Load(path);
                TorrentManager torrentManager = new TorrentManager(torrent, SaveFolder, new TorrentSettings());
                engine.Register(torrentManager);
                System.Threading.Tasks.Task task = engine.StartAllAsync();
                task.Wait();

                // Keep running while the torrent isn't stopped or paused.
                while (!IsStopped && torrentManager.State != TorrentState.Stopped && torrentManager.State != TorrentState.Paused)
                {
                    Thread.Sleep(1000);

                    if (torrentManager.Progress == 100.0)
                    {
                        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                        //torrentManager.Stop();
                        engine.StopAll();
                        break;
                    }
                }

                InfoFormat("The torrent {0} download succeeded.", path);
                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading the torrent {0}: {1}", path, e.Message);
                return false;
            }
        }

    }
}
