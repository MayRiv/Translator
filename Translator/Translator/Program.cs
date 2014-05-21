using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LexicalAnalyzerLibrary;
using SyntaxAnalyzerAutomatLibrary;

namespace Translator
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 view = new Form1();
            TranslatorPresenter presenter = new TranslatorPresenter(view);
            Application.Run(view);
        }
    }
}
