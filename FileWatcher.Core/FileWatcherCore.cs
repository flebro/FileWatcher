using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Core
{
    public class FileWatcherCore
    {
        #region Fields

        private string _InputDirectoryPath;
        private string _OutputDirectoryPath;
        private FileSystemWatcher _FileSystemWatcher;

        #endregion

        #region Properties

        /// <summary>
        /// Obtient ou définit le répertoire à écouter.
        /// </summary>
        public string InputDirectoryPath
        {
            get { return _InputDirectoryPath; }
            private set { _InputDirectoryPath = value; }
        }

        /// <summary>
        ///     Obtient ou définit le répertoire de sortie.
        /// </summary>
        public string OutputDirectoryPath
        {
            get { return _OutputDirectoryPath; }
            private set { _OutputDirectoryPath = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FileWatcher.Test.FileWatcherCore"/>.
        /// </summary>
        /// <param name="inputDirectoryPath">Répertoire à écouter.</param>
        /// <param name="outputDirectoryPath">Répertoire de sortie.</param>
        public FileWatcherCore(string inputDirectoryPath, string outputDirectoryPath)
        {
            //ExpandEnvironmentVariables permet de résoudre les chemins comme %temp%
            InputDirectoryPath = Environment.ExpandEnvironmentVariables(inputDirectoryPath);
            OutputDirectoryPath = Environment.ExpandEnvironmentVariables(outputDirectoryPath);

            #region Arguments Testing

            if (string.IsNullOrWhiteSpace(InputDirectoryPath))
            {
                //throw new ArgumentException("Le paramètre " + nameof(inputDirectoryPath) 
                //    + " n'est pas défini ou vide."
                //    , nameof(inputDirectoryPath));

                throw new ArgumentException(
                    $"Le paramètre {nameof(inputDirectoryPath)} n'est pas défini ou vide."
                    , nameof(inputDirectoryPath));

                //$ devant une chaîne appel la méthode string.Format
                //string.Format("Le paramètre {0} n'est pas défini ou vide.", nameof(inputDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(OutputDirectoryPath))
            {
                throw new ArgumentException(
                $"Le paramètre {nameof(outputDirectoryPath)} n'est pas défini ou vide."
                , nameof(outputDirectoryPath));
            }

            #endregion

            #region Directories Check

            //Permet de vérifier si le dossier existe.
            if (!Directory.Exists(InputDirectoryPath))
            {
                //Crée tous les répertoires et sous-répertoires dans le chemin d'accès spécifié, sauf s'ils existent déjà.
                Directory.CreateDirectory(InputDirectoryPath);
            }

            //CreateDirectory ne créé pas le(s) dossier(s) s'il(s) existe(nt) déjà.
            Directory.CreateDirectory(OutputDirectoryPath);

            #endregion

            _FileSystemWatcher = new FileSystemWatcher(InputDirectoryPath);
            _FileSystemWatcher.IncludeSubdirectories = false;

            _FileSystemWatcher.Created += _FileSystemWatcher_Created;
        }

        #endregion

        #region Methods

        #region FileSystemWatcher Events

        private void _FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //Petite temporisation
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("-------");
            Console.WriteLine("CREATED");
            Console.WriteLine("FullPath : " + e.FullPath);
            Console.WriteLine("-------");

            if (File.Exists(e.FullPath))
            {
                WaitFileCopied(e.FullPath);
                File.Move(e.FullPath, e.FullPath.Replace(InputDirectoryPath, OutputDirectoryPath));
                Console.WriteLine("Moved");
            }
            else if(Directory.Exists(e.FullPath))
            {
                WaitDirectoryCopied(e.FullPath);
                Directory.Move(e.FullPath, e.FullPath.Replace(InputDirectoryPath, OutputDirectoryPath));
                Console.WriteLine("Moved");
            }
        }

        #endregion

        #region Service Management

        public void Start()
        {
            _FileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _FileSystemWatcher.EnableRaisingEvents = false;
        }

        public void Pause()
        {
            _FileSystemWatcher.EnableRaisingEvents = false;
        }

        public void Resume()
        {
            _FileSystemWatcher.EnableRaisingEvents = true;
        }

        #endregion

        public void WaitDirectoryCopied(string directoryPath)
        {
            bool directoryCopied = false;
            bool subFilesCopied = false;
            bool subDirectoryCopied = false;

            do
            {
                foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories))
                {
                    DateTime lastWriteTime = Directory.GetLastWriteTime(subDirectoryPath);

                    if(lastWriteTime.AddMinutes(1) > DateTime.Now)
                    {
                        subDirectoryCopied = false;
                        Console.WriteLine("subDirectory not copied");
                        System.Threading.Thread.Sleep(500);
                        break;
                    }
                    else
                    {
                        subDirectoryCopied = true;
                        Console.WriteLine("subDirectoryCopied");
                    }
                }

                if (subDirectoryCopied)
                {
                    foreach (string filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                    {
                        if (!CheckFileCopied(filePath))
                        {
                            subFilesCopied = false;
                            Console.WriteLine("subFiles not copied");
                            System.Threading.Thread.Sleep(500);
                            break;
                        }
                        else
                        {
                            subFilesCopied = true;
                            Console.WriteLine("subFilesCopied");
                        }
                    }
                }

                directoryCopied = subDirectoryCopied && subFilesCopied;
            } while (!directoryCopied);
        }

        public void WaitFileCopied(string filePath)
        {
            while (!CheckFileCopied(filePath))
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public bool CheckFileCopied(string filePath)
        {
            FileStream fs = null;
            bool fileOpened = false;

            try
            {
                //Tentative d'ouverture du fichier en lecture / écriture en mode exclusif
                fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                //Si on a pas d'exception, le fichier est bien ouvert et accessible.
                fileOpened = true;
                //On ferme le fichier.
                fs.Close();
                //On désafecte la référence du FileStream.
                fs = null;
            }
            catch (Exception)
            {
                //Le fichier n'est pas accessible
            }

            return fileOpened;
        }

        #endregion
    }
}
