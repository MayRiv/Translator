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
            //LexicalAnalyzer lanalyzer = new LexicalAnalyzer(@"lexemesJeka.txt", @"separators.txt");
            //lanalyzer.ReadSourceCodeFile(@"reduceProgram.txt");
            
            ////lanalyzer.OutInputData();
            //if (lanalyzer.Analyze())
            //{
            //    //lanalyzer.OutLexemes();
            //    //lanalyzer.OutConstAndIdentifierTable();
            //    LexemeCodeHelper helper = new LexemeCodeHelper(lanalyzer.getLexemesTable());
            //    SyntaxAnalyzerAutomat s = new SyntaxAnalyzerAutomat("syntax_states2.xml", lanalyzer.getOutputLexems(), new LexemeCodeHelper(lanalyzer.getLexemesTable()));

            //    s.analyze();
         
            //}
            //Console.Read();
        
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 view = new Form1();
            TranslatorPresenter presenter = new TranslatorPresenter(view);
            Application.Run(view);
        }
    }
}
