﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LexicalAnalyzerLibrary;
using SyntaxAnalyzerAutomatLibrary;
using PolizGenerator;
using PolizInterpretator;
namespace Translator
{
    class TranslatorPresenter
    {
        private ITranslatorView view;
        private LexicalAnalyzer lanalyzer;
        
        private SyntaxAnalyzerAutomat sanalyzer;

        Thread thread;
        public TranslatorPresenter(ITranslatorView view)
        {
            this.view = view;
            view.TryAnalyze += view_TryAnalyze;
        }

        void view_TryAnalyze(object sender, EventArgsTranslator e)
        {
            thread = new Thread(Analyze);
            thread.Start(e);
        }
        void Analyze(object o)
        {
            try
            {
                EventArgsTranslator e = o as EventArgsTranslator;
                lanalyzer = new LexicalAnalyzer(e.PathToLexemes, e.PathToSeparators,view);
                lanalyzer.ReadSourceCode(e.Source);
                ////lanalyzer.OutInputData();
                if (lanalyzer.Analyze())
                {
                    lanalyzer.OutLexemes();
                    lanalyzer.OutConstAndIdentifierTable();
                    sanalyzer = new SyntaxAnalyzerAutomat(e.PathToSyntaxRules, lanalyzer.getOutputLexems(), new LexemeCodeHelper(lanalyzer.getLexemesTable()));
                    ((ILexicalOutputer)view).Output(Environment.NewLine);
                    sanalyzer.analyze();
                    ((ILexicalOutputer)view).Output("Syntax analyze has been succesful!" + Environment.NewLine);
                    POLIZGenerator plGenerator = new POLIZGenerator(lanalyzer.outputLexemesTable, lanalyzer.getLexemesTable(), e.PathToTableOfPriority, e.PathToLexemeDescription);

                    plGenerator.GeneratePOLIZ();

                    string poliz = plGenerator.GetPolizAsString();
                    
                    PolizExecuter executer = new PolizExecuter(poliz, lanalyzer.identifierTable, lanalyzer.constTable, view);
                    //view.Output(poliz);
                    executer.Execute();
                }
            }
            catch (Exception ex)
            {
                //view.Result = ex.Message;
                ((ILexicalOutputer)view).Output(ex.Message);
            }
        }
    }
}
