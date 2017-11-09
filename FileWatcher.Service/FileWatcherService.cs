using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using FileWatcher.Core;

namespace FileWatcher.Service
{
    public partial class FileWatcherService : ServiceBase
    {
        #region Fields

        private FileWatcherCore _FileWatcherCore;

        #endregion

        #region Contructors

        public FileWatcherService()
        {
            InitializeComponent();
            _FileWatcherCore = new FileWatcherCore(@"%temp%\FileWatcher\Input", @"%temp%\FileWatcher\Output");
        }

        #endregion

        #region Methods

        protected override void OnPause() =>_FileWatcherCore.Pause();

        protected override void OnContinue() => _FileWatcherCore.Resume();

        protected override void OnStart(string[] args) => _FileWatcherCore.Start();

        protected override void OnStop() => _FileWatcherCore.Stop();
        
        #endregion
    }
}
