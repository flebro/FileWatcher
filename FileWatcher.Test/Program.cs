using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileWatcher.Core;

namespace FileWatcher.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            FileWatcherCore watcher = new FileWatcherCore(@"%temp%\FileWatcher\Input", @"%temp%\FileWatcher\Output");

            //System.Diagnostics.Process.Start(Environment.ExpandEnvironmentVariables(@"%temp%\FileWatcher\Input"));
            //System.Diagnostics.Process.Start(Environment.ExpandEnvironmentVariables(@"%temp%\FileWatcher\Output"));

            watcher.Start();

            Console.ReadKey();
        }
    }
}
