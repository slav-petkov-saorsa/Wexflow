// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Raven.Client.ServerWide.Operations.Certificates;
using System.Diagnostics;
using System.Text;

namespace System.IO
{
 
    public struct FileChange
    {
        public static bool IsFileLocked(string FilePath)
        {
            try
            {
                using (Stream stream = new FileStream(FilePath, FileMode.Open))
                {
                    stream.Close();
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }     
        internal FileChange(string directory, string path, WatcherChangeTypes type)
        {
            Debug.Assert(path != null);
            Directory = directory;
            Name = path;
            ChangeType = type;
            FileLocked = IsFileLocked(Path.Combine(directory, path));
        }

        public string Directory { get; }
        public string Name { get; }
        public WatcherChangeTypes ChangeType { get; }
        public bool FileLocked { get; }
    }
}    
