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
        //Depuis C# 7
        //public string OutputDirectoryPath
        //{
        //    get => _OutputDirectoryPath;
        //    private set => _OutputDirectoryPath = value;
        //}

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

            Log.LogText(Environment.CurrentDirectory);
            Log.LogText(InputDirectoryPath);
            Log.LogText(OutputDirectoryPath);

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

            Log.LogText("FileWatcherCore Init ended");
        }

        #endregion

        #region Methods

        #region FileSystemWatcher Events

        private void _FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //Petite temporisation
            System.Threading.Thread.Sleep(1000);

            Log.LogText("-------");
            Log.LogText("CREATED");
            Log.LogText("FullPath : " + e.FullPath);
            Log.LogText("-------");

            if (File.Exists(e.FullPath))
            {
                Log.LogText("Is a file");
                WaitFileCopied(e.FullPath);
                Log.LogText("File copied");
                File.Move(e.FullPath, e.FullPath.Replace(InputDirectoryPath, OutputDirectoryPath));
                Log.LogText("Moved");
            }
            else if (Directory.Exists(e.FullPath))
            {
                Log.LogText("Is a directory");
                WaitDirectoryCopied(e.FullPath);
                Log.LogText("Directory copied");
                Directory.Move(e.FullPath, e.FullPath.Replace(InputDirectoryPath, OutputDirectoryPath));
                Log.LogText("Moved");
            }
        }

        #endregion

        #region Service Management

        public void Start()
        {
            if (_FileSystemWatcher == null)
            {
                _FileSystemWatcher = new FileSystemWatcher(InputDirectoryPath);
                _FileSystemWatcher.IncludeSubdirectories = false;
                _FileSystemWatcher.Created += _FileSystemWatcher_Created;
            }
            _FileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            //if (_FileSystemWatcher != null)
            //{
            //    _FileSystemWatcher.Dispose();
            //    _FileSystemWatcher = null;
            //}

            //Depuis C# 6 (VS 2015), il est possible d'appeler une méthode si et seulement si la référence n'est pas null.
            _FileSystemWatcher?.Dispose();
            _FileSystemWatcher = null;
        }

        public void Pause()
        {
            if (_FileSystemWatcher != null)
            {
                _FileSystemWatcher.EnableRaisingEvents = false;
            }
        }

        //Depuis C# 7 on peut uiliser l'opérateur => si la méthode n'a qu'une seule instruction
        public void Resume() => Start();

        #endregion

        #region File Sytem Methods

        void WaitDirectoryCopied(string directoryPath)
        {
            //Pour vérifier si un dossier est copié,
            //on vérifie que pour chaque sous-dossier le dernier accès en écriture est supérieur à 1 minutes.
            //si c'est le cas, on vérifie que chaque fichier est bien accessible en lecture/écriture avec un vérou exclusif.
            //A la moindre erreur, on recommence l'ensemble des vérifications.

            bool directoryCopied = false;
            bool subFilesCopied = false;
            bool subDirectoryCopied = false;

            do
            {
                //Si on a pas de sous-dossier, on passe subDirectoryCopied à true
                subDirectoryCopied = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories).Any() == false;

                //Si on a des sous-dossiers, on fait la boucle de vérification des sous-dossiers
                if (!subDirectoryCopied)
                {
                    foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories))
                    {
                        DateTime lastWriteTime = Directory.GetLastWriteTime(subDirectoryPath);

                        if (lastWriteTime.AddMinutes(1) > DateTime.Now)
                        {
                            subDirectoryCopied = false;
                            Log.LogText("subDirectory not copied");
                            System.Threading.Thread.Sleep(500);
                            break;
                        }
                        else
                        {
                            subDirectoryCopied = true;
                            Log.LogText("subDirectoryCopied");
                        }
                    }
                }

                //S'il n'existe pas de sous-dossier ou s'ils sont bien copiés, on fait la boucle de vérification des fichiers.
                if (subDirectoryCopied)
                {
                    //Si on a pas de fichiers dans le dossier, on passe subFilesCopied à true
                    subFilesCopied = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories).Any() == false;

                    //Si on a des fichiers, on fait la boucle de vérification des sous-dossiers
                    if (!subFilesCopied)
                    {
                        foreach (string filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                        {
                            if (!CheckFileCopied(filePath))
                            {
                                subFilesCopied = false;
                                Log.LogText("subFiles not copied");
                                System.Threading.Thread.Sleep(500);
                                break;
                            }
                            else
                            {
                                subFilesCopied = true;
                                Log.LogText("subFilesCopied");
                            }
                        }
                    }
                }

                directoryCopied = subDirectoryCopied && subFilesCopied;
            } while (!directoryCopied);
        }

        void WaitFileCopied(string filePath)
        {
            while (!CheckFileCopied(filePath))
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        bool CheckFileCopied(string filePath)
        {
            bool fileOpened = false;

            try
            {
                //IDisposable est une interface qui founie un mécanisme de supresion des objets non managés.
                //IDisposable définie une méthode Dispose qui doit être appelée pour nettoyer les ressources non managés.
                //Si la méthoe n'est pas appelé, il ya un risque de fuite mémoire.
                //L'instruction using permet de gérer convenablement les objets IDisposable
                //Dans l'instruction using, il est important de déclarer la référence et obligaoire d'instancier l'objet.
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    //Si on a pas d'exception, le fichier est bien ouvert et accessible.
                    fileOpened = true;
                }
            }
            catch (Exception)
            {
                //Le fichier n'est pas accessible
            }

            return fileOpened;
        }

        #endregion

        #endregion
    }
}