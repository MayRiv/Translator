﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
namespace LexicalAnalyzerLibrary
{
    public class LexicalAnalyzer
    {
        private List<string> lexemesTable;
        private List<string> separatorsTable;
        private List<string> sourceCode;

        public List<LexemeLine> outputLexemesTable;
        public List<double> constTable;
        public List<string> identifierTable;
        public List<LexemeLine> getOutputLexems() { return outputLexemesTable; }
        public List<string> getLexemesTable() { return lexemesTable; }
        private ILexicalOutputer view;
        private static bool IsIdentifier(string identifier)
        {
            Regex identifierExp = new Regex(@"^([a-z])([a-z0-9])*$", RegexOptions.IgnoreCase);//@"^([\w-[\d]])([\w-[_]])*$"
            return identifierExp.IsMatch(identifier);
        }
        private static bool IsConst(string constant)
        {
            double t;
            return double.TryParse(constant, NumberStyles.Float, CultureInfo.InvariantCulture, out t);
        }
        private static bool IsNotExp(string expStart)
        {
            return !IsConst(expStart + "+1");
        }

        private void ReadLexemTable(string fileName)
        {
            lexemesTable.AddRange(File.ReadAllLines(fileName));
        }
        private void ReadSeparatorTable(string fileName)
        {
            separatorsTable.AddRange(File.ReadAllLines(fileName));
        }
        public void ReadSourceCode(string[] code)
        {
            sourceCode.AddRange(code);
            while (sourceCode.Remove(""))
            {
            }
            for (int i = 0; i < sourceCode.Count; i++) //There was from i = 1
            {
                sourceCode[i] += "?";
            }
        }
        public void ReadSourceCodeFile(string fileName)
        {

            ReadSourceCode(File.ReadAllLines(fileName));
        }
        public void OutInputData()
        {
            Console.WriteLine("Input lexemes table:");
            foreach (string str in lexemesTable)
            {
                Console.WriteLine(str);
            }
            Console.WriteLine("Input separators table:");
            foreach (string str in separatorsTable)
            {
                Console.WriteLine(str);
            }
            Console.ReadLine();
        }
        public void OutLexemes()
        {
            Console.WriteLine("Output lexemes table:");
            foreach (LexemeLine str in outputLexemesTable)
            {
                str.Display();
            }
            Console.ReadLine();
        }
        public void OutConstAndIdentifierTable()
        {
             view.Output(string.Format("Output identifier table:"));
            for (int i = 0; i < identifierTable.Count; i++)
            {
                 view.Output(string.Format("{0:D2} | {1}", i, identifierTable[i]));
            }
             view.Output(string.Format("Output constant table:"));
            for (int i = 0; i < constTable.Count; i++)
            {
                 view.Output(string.Format("{0:D2} | {1}", i, constTable[i]));
            }
        }

        public LexicalAnalyzer(string lexemesTablePath, string separatorsTablePath, ILexicalOutputer view)
        {
            this.view = view;
            lexemesTable = new List<string>();
            separatorsTable = new List<string>();
            sourceCode = new List<string>();

            ReadLexemTable(lexemesTablePath);
            ReadSeparatorTable(separatorsTablePath);

            outputLexemesTable = new List<LexemeLine>();
            constTable = new List<double>();
            identifierTable = new List<string>();
        }
        private string takeFirstWord(string line)
        {
            char[] separator = new char[1];
            separator[0] = ' ';
            string[] words = line.Split(separator);
            if (words[0].Last() == '?') return words[0].Remove(words[0].Length - 1);
            return words[0];
        }
        private string removeFirstWord(string line)
        {
            char[] separator = new char[1];
            separator[0] = ' ';
            string[] words = line.Split(separator);
            string result = "";
            for (int i = 1; i < words.GetLength(0); i++)
            {
                result = string.Concat(result, words[i]);
                result = string.Concat(result, " ");
            }
            if (result.Length > 0) return result.Remove(result.Length - 1);
            else return "?";
        }
        public bool Analyze()
        {
            int identifierCode = lexemesTable.Count;
            int constantCode = lexemesTable.Count + 1;
            string buffer = "";
            bool declarationsSection = false;
            bool isAnalysisTrue = true;

            //if (IsIdentifier(sourceCode[0]))
            if (IsIdentifier(takeFirstWord(sourceCode[0])))
            {
                outputLexemesTable.Add(new LexemeLine(0, "id: " + takeFirstWord(sourceCode[0]), identifierCode, 0));
                identifierTable.Add(takeFirstWord(sourceCode[0]));
                sourceCode[0] = removeFirstWord(sourceCode[0]);
            }


            for (int lineN = 0; lineN < sourceCode.Count; lineN++) //There was from 1
            {
                string str = sourceCode[lineN];
                for (int i = 0; i < str.Length; i++)
                {
                    if (separatorsTable.Contains(str[i].ToString()) || (((str[i].ToString() == "+") || str[i].ToString() == "-") && IsNotExp(buffer)))
                    {
                        int lexemN = lexemesTable.IndexOf(buffer); //Проверяем буфер в таб лексем
                        if (lexemN >= 0)
                        {
                            outputLexemesTable.Add(new LexemeLine(lineN + 1, buffer, lexemN, null)); //если он там есть то заносим буфер в выходную таблицу лексем
                            if (buffer == lexemesTable[0])
                            {
                                declarationsSection = true;
                            }
                            if (buffer == lexemesTable[1])
                            {
                                declarationsSection = false;
                            }
                        }
                        else
                        {
                            if (IsConst(buffer))
                            {
                                if (!constTable.Contains((double.Parse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture))))
                                {
                                    constTable.Add(double.Parse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture));
                                }
                                outputLexemesTable.Add(new LexemeLine(lineN + 1, buffer, constantCode, constTable.Count));
                            }
                            else
                            {
                                if (declarationsSection)
                                {
                                    if (IsIdentifier(buffer))
                                    {
                                        if (identifierTable.Contains(buffer))
                                        {
                                            
                                            view.Output(string.Format("Line: {0} Error: Redeclaring identifier: {1}", lineN, buffer));
                                            isAnalysisTrue = false;
                                        }
                                        else
                                        {
                                            outputLexemesTable.Add(new LexemeLine(lineN + 1, buffer, identifierCode, identifierTable.Count));
                                            identifierTable.Add(buffer);
                                        }
                                    }
                                    else
                                    {
                                        if (str[i] != ',') // Жуткий костыль, призванный исправить специфический случай в грамматике, необходимый для reduceAnalizer. Когда у меня два сператора под ряд идут, лексический анализатор падает. Пример - " ,"
                                        {
                                             view.Output(string.Format("Line: {0} Error: Unknown expression: {1}", lineN, buffer));
                                            isAnalysisTrue = false;
                                        }
                                    }
                                }
                                else
                                {
                                    lexemN = identifierTable.IndexOf(buffer);
                                    if (lexemN >= 0)
                                    {
                                        outputLexemesTable.Add(new LexemeLine(lineN + 1, buffer, identifierCode, lexemN));
                                    }
                                    else
                                    {
                                        if (buffer != "")
                                        {
                                            if (IsIdentifier(buffer))
                                            {
                                                view.Output(string.Format("Line: {0} Error: Undeclared identifier: {1}", lineN, buffer));
                                                isAnalysisTrue = false;
                                            }
                                            else
                                            {
                                                 view.Output(string.Format("Line: {0} Error: Unknown expression: {1}", lineN, buffer));
                                                isAnalysisTrue = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        lexemN = lexemesTable.IndexOf(str[i].ToString());   //проверяем есть ли разделитель в таблице лексем
                        if (lexemN >= 0)
                        {
                            outputLexemesTable.Add(new LexemeLine(lineN + 1, str[i].ToString(), lexemN, null)); //если есть то заносим в выходную таблицу
                        }
                        buffer = "";    //очищаем буфер
                    }
                    else
                    {
                        buffer += str[i];
                    }
                }
            }
            if (buffer.Length != 0)
            {
                int lexemN = lexemesTable.IndexOf(buffer); //Проверяем буфер в таб лексем
                if (lexemN >= 0)
                {
                    outputLexemesTable.Add(new LexemeLine(sourceCode.Count, buffer, lexemN, null)); //если он там есть то заносим буфер в выходную таблицу лексем

                }
                else if (IsConst(buffer))
                {
                    if (!constTable.Contains((double.Parse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture))))
                    {
                        constTable.Add(double.Parse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture));
                    }
                    outputLexemesTable.Add(new LexemeLine(sourceCode.Count, buffer, constantCode, constTable.Count));
                }
                else if (IsIdentifier(buffer))
                {
                    lexemN = identifierTable.IndexOf(buffer);
                    if (lexemN >= 0)
                    {
                        outputLexemesTable.Add(new LexemeLine(sourceCode.Count, buffer, identifierCode, lexemN));
                    }
                }
                else throw new Exception("Garbage at the end of source code, lexical analizer couldn't analize it");
            }




            //lexemesTable.Add("id");
            //lexemesTable.Add("const");



            if (isAnalysisTrue)
            {
                view.Output(string.Format("Lexical analysis was successful !"));
                return true;
            }
            else
            {
                view.Output(string.Format("Lexical analysis was UNsuccessful !"));
                return false;
            }






        }

    }

}
