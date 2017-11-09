using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Core
{
    static class Log
    {
        #region Fields

        private static string _LogFilePath;

        #endregion

        #region Constructors

        //Il est possible de créer un constructeur static.
        //Ils sont appelés automatiquement une seule fois lors de la première utilisation de la classe.
        //Ils servent principalement à instancier les membres static.
        static Log()
        {
            _LogFilePath = @"C:\log.txt";
            if (File.Exists(_LogFilePath))
            {
                File.Delete(_LogFilePath);
            }
        }

        #endregion

        static internal void LogText(string text)
        {
            #region Pour info
            /*
            Il es posible de faire de la compilaion conditionnelle avec des symboles de compilation
            Par défaut, le symbole DEBUG est défini lorsque la coniguration de génération est en Debug
            Il est possible de créer ses propres configuration de génération, ainsi qu'en dupliquer une.
            Ici, j'ai créé à partir de la configuration Debug la configuration Debug.Console
            Dans les propriétés du projet FileWatcher.Core, onglet Build, j'ai ajouté le symbole DEBUGCONSOLE
            pour la configuration Debug.Console
            les instructions de préprocesseurs #if, #elif, #else et #end sont utilisés pour réalier de la compilation conditionnelle.
            */
            #endregion

            text = $"[{DateTime.Now}]" + text;

#if DEBUGCONSOLE
            Console.WriteLine(text);
#elif DEBUG
            File.AppendAllText(_LogFilePath, text + Environment.NewLine);
#else
            //NOP;
#endif
        }
    }
}
