using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.FileSystem.Completion
{
    public class PathCompletor
    {
        private static ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
        private static bool canThread = true;
        private ConcurrentDictionary<string, string[]> paths; 

        public PathCompletor(DriveInfo drive)
        {
            LoadPaths(drive);
        }

        private void LoadPaths(DriveInfo drive)
        {
            string driveName = drive.Name;
            LoadPath(driveName);
        }

        private void LoadPath(string path, uint depth = 0)
        {
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            string[] joins = new string[directories.Length + files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                joins[i] = files[i];
            }
            for (int i = 0; i < directories.Length; i++)
            {
                joins[i] = directories[i];
            }

            paths.(path, joins);

            if (canThread)
            {
                for (int i = 0; i < directories.Length; i++)
                {

                }
            }
            else
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    LoadPath(directories[i]);
                }
            }
        }
    }
}
