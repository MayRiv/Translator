using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LexicalAnalyzerLibrary;
namespace PolizGenerator
{
    public struct LexemeDescription
    {
        public bool isWriteStack;
        public bool isWritePoliz;
        public int labelNumber;
        public int cellNumber;
        public string poliz;
        public string clearTo;

        public LexemeDescription(bool writeStack, bool writePoliz, int labelN, int cellN, string pol, string clear)
        {
            isWriteStack = writeStack;
            isWritePoliz = writePoliz;
            labelNumber = labelN;
            cellNumber = cellN;
            poliz = pol;
            clearTo = clear;
        }
    }

    public class POLIZGenerator
    {
        private List<LexemeLine> lexemeTable;
        private List<string> inputLexemes;
        private Dictionary<string, int> prioritiesTable = new Dictionary<string,int>();
        private Dictionary<string, LexemeDescription> specialLexDescription = new Dictionary<string,LexemeDescription>();

        private List<double> cellStack;
        private int lastCell = 0;
        private Stack<int> labelStack;
        private int lastLabel;
        
        private string lastAssignIdentifier = "";      
        private string assignSign;
        private string inputSign;
        private string outputSign;
        private string operatorSeparator;

        #region condition description part

        Dictionary<string, LexemeDescription> conditionalDescriptions = new Dictionary<string, LexemeDescription>();
        List<string> conditionLexemesList = new List<string>();
        bool isCheckConditions = false;

        #endregion

        private List<string> poliz;
        private Stack<string> stack;

        #region initializing reading methods
        private void ReadPrioritiesTable(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList priorities = xmlDoc.SelectSingleNode("priorities").SelectNodes("priority");
            
            for(int i=0; i<priorities.Count; i++)
            {
                string name = priorities[i].Attributes[0].InnerText;
                int pr = int.Parse(priorities[i].Attributes[1].InnerText);
                prioritiesTable.Add(name, pr);          
            }       
        }

        private LexemeDescription ReadSingleLexemeDescription(XmlNode description)
        {
            bool isWrSt = bool.Parse(description.SelectSingleNode("iswrstack").InnerText);
            bool isWrPl = bool.Parse(description.SelectSingleNode("iswrpoliz").InnerText);
            int ln = int.Parse(description.SelectSingleNode("labelnumber").InnerText);
            int cn = int.Parse(description.SelectSingleNode("cellnumber").InnerText);
            string pol = description.SelectSingleNode("poliz").InnerText;
            string clr = description.SelectSingleNode("clearto").InnerText;
            if (clr == "null")
            {
                clr = null;
            }
            else
            {
                if (bool.Parse(description.SelectSingleNode("clearto").Attributes[0].InnerText) == true)
                {
                    clr += " included";
                }
            }
            if (pol == "null")
            {
                pol = null;
            }
            return new LexemeDescription(isWrSt, isWrPl, ln, cn, pol, clr);
        }
        private void ReadLexemeDescription(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode lexemesDescription = xmlDoc.SelectSingleNode("lexemedescription");

            XmlNode assign = lexemesDescription.SelectSingleNode("assign");
            assignSign = assign.InnerText;
            XmlNode input = lexemesDescription.SelectSingleNode("in");
            inputSign = input.InnerText;
            XmlNode output = lexemesDescription.SelectSingleNode("out");
            outputSign = output.InnerText;
            XmlNode opsep = lexemesDescription.SelectSingleNode("operatorsep");
            operatorSeparator = opsep.InnerText;
            XmlNodeList lexemesDescriptionList = lexemesDescription.SelectNodes("lexeme");
            for (int i = 0; i < lexemesDescriptionList.Count; i++)
            {
                if (lexemesDescriptionList[i].HasChildNodes)
                {
                    string key = lexemesDescriptionList[i].Attributes[0].InnerText;
                    LexemeDescription value = ReadSingleLexemeDescription(lexemesDescriptionList[i]);
                    specialLexDescription.Add(key, value);
                }
            }

            XmlNodeList condLexemes = lexemesDescription.SelectNodes("condlexeme");
            for (int i = 0; i < condLexemes.Count; i++)
            {
                if (condLexemes[i].HasChildNodes)
                {
                    string key = condLexemes[i].Attributes[0].InnerText;
                    conditionLexemesList.Add(condLexemes[i].Attributes[1].InnerText);
                    LexemeDescription value = ReadSingleLexemeDescription(condLexemes[i]);
                    conditionalDescriptions.Add(key, value);
                }
            }
        }
        #endregion

        #region special lexeme description processing methods
        private void ProcessSpecialLexDescription(LexemeDescription currDesc, string lexeme)
        {
            if(currDesc.clearTo != null)
            {
                string[] twoParts = currDesc.clearTo.Split(' ');
                string clearTarget = twoParts[0];
                bool included = false;
                if (twoParts.Length == 2 && twoParts[1] == "included")
                {
                    included = true;
                }
                while(stack.Count !=0 && stack.Peek() != clearTarget)//TODO поставить проверку на бесконечность цикла
                {
                    PopAndPushToPoliz();  //TODO оптимизировать кол-во ифов
                }
                if(included)
                {
                    if (stack.Count != 0 && stack.Peek() == clearTarget)
                    {
                        PopAndPushToPoliz();
                    }
                }
            }
            if (currDesc.isWriteStack)
            {
                stack.Push(lexeme);
            }
            if (currDesc.labelNumber > 0)
            {
                int newLabelN = lastLabel + currDesc.labelNumber;      
                lastLabel = newLabelN;                
                labelStack.Push(newLabelN);
            }
            if (currDesc.cellNumber > 0)
            {
                lastCell = lastCell + currDesc.cellNumber;
                while(lastCell>cellStack.Count)
                {
                    cellStack.Add(0.0);
                }          
            }
            if(currDesc.poliz!=null)
            {
                LexDesPolizParser(currDesc.poliz);
            }
        }
        private void LexDesPolizParser(string pol)
        {
            string[] polizParts = pol.Split(' ');
            for (int i = 0; i < polizParts.Length; i++)
            {
                string pl = polizParts[i];
                if (pl.Contains("label"))
                {
                    int temp1 = labelStack.Peek() - int.Parse(pl.Substring(5));
                    polizParts[i] = "m" + temp1.ToString();
                }
                else
                    if (pl == "popl")
                    {
                        labelStack.Pop();
                        polizParts[i] = "";
                    }
                    else
                        if (pl.Contains("cell"))
                        {
                            int temp1 = lastCell - int.Parse(pl.Substring(4)) - 1;
                            polizParts[i] = "_$c" + temp1;
                        }
                        else
                            if (pl.Contains("freec"))
                            {
                                int temp1 = int.Parse(pl.Substring(5));
                                lastCell -= temp1;
                                polizParts[i] = "";
                            }
                            else
                                if (pl.Contains("popid"))
                                {
                                    polizParts[i] = lastAssignIdentifier;
                                }
            }
            pol = "";
            for (int i = 0; i < polizParts.Length; i++)
            {
                if (polizParts[i] != "")
                {               
                    poliz.Add(polizParts[i]);                  
                }
            }
        }
        private void PopAndPushToPoliz()
        {
            if(stack.Count !=0)
            {
                string currLex = stack.Pop();
                if(specialLexDescription.ContainsKey(currLex))
                {
                    if(specialLexDescription[currLex].isWritePoliz)
                    {
                        poliz.Add(currLex);
                    }
                }
                else
                {
                    poliz.Add(currLex);
                }
            }
        }

        #endregion

        #region constructor
        public POLIZGenerator(List<LexemeLine> lexemTable, List<string> lexemes,/* Dictionary<string, double> idTable, Dictionary<string, double> consTable,*/ string priorityTablePath, string lexemeDescriptionPath)
        {
            ReadPrioritiesTable(priorityTablePath);
            ReadLexemeDescription(lexemeDescriptionPath);
            lexemeTable = lexemTable;
            /*identifierTable = idTable;
            constTable = consTable;*/
            inputLexemes = lexemes;

            cellStack = new List<double>();
            labelStack = new Stack<int>();
            labelStack.Push(0);
            poliz = new List<string>();
            stack = new Stack<string>();
        }
        #endregion

        public void ShowPoliz()
        {
            PreparePOLIZ();
            foreach (var item in poliz)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();

        }
        public string GetPolizAsString()
        {
            PreparePOLIZ();
            StringBuilder sb = new StringBuilder();
            foreach (var item in poliz)
            {
                sb = sb.Append(item);
                sb = sb.Append(" ");
                
            }
            return sb.ToString();
            
        }
        public void GeneratePOLIZ()
        {
            int startSymbolN = 0;
            int finishSymbol = lexemeTable.Count - 1;

            while (lexemeTable[startSymbolN].lexemeCode != 1)
            {
                startSymbolN++;
            }
                startSymbolN += 1;

            while (lexemeTable[finishSymbol].lexemeCode != 2)
            {
                finishSymbol--;
            }

            for (int i = startSymbolN; i < finishSymbol; i++)
            {
                if (lexemeTable[i].lexemeCode == inputLexemes.Count - 1 || lexemeTable[i].lexemeCode == inputLexemes.Count - 2) //проверка на ид и конст
                {
                    poliz.Add(lexemeTable[i].lexeme);
                }
                else
                {
                    //TODO writing in lastAssignIdentifier last identifier
                    if(lexemeTable[i].lexeme==assignSign)
                    {
                        lastAssignIdentifier = poliz[poliz.Count-1];
                    }
                    if (conditionLexemesList.Contains(lexemeTable[i].lexeme))
                    {
                        isCheckConditions = true;
                    }
                    if (specialLexDescription.ContainsKey(lexemeTable[i].lexeme))
                    {
                        ProcessSpecialLexDescription(specialLexDescription[lexemeTable[i].lexeme], lexemeTable[i].lexeme);
                    }
                    else

                        if (isCheckConditions && conditionalDescriptions.ContainsKey(lexemeTable[i].lexeme))
                        {
                            ProcessSpecialLexDescription(conditionalDescriptions[lexemeTable[i].lexeme], lexemeTable[i].lexeme);
                            isCheckConditions = false;
                        }
                        else
                        {
                            try
                            {
                                while (stack.Count != 0 && prioritiesTable[stack.Peek()] >= prioritiesTable[lexemeTable[i].lexeme])
                                {
                                    PopAndPushToPoliz();
                                }
                                stack.Push(lexemeTable[i].lexeme);
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }

                }
            }
        }

        #region execute poliz methods
        private void PreparePOLIZ()
        {
            //TODO task1: delete all enters from poliz
            //TODO task2: all symbols : remove to previous poliz element
            List<string> changedPoliz = new List<string>(); 
            for (int i = 0; i < poliz.Count; i++)
            {
                if(poliz[i]!=operatorSeparator)
                {
                    if(poliz[i]==":")
                    {
                        changedPoliz[changedPoliz.Count - 1] += ":";
                    }
                    else
                    {
                        changedPoliz.Add(poliz[i]);
                    }                    
                }
            }
            poliz = changedPoliz;
        }
        public void ExecutePOLIZ()
        {
            PreparePOLIZ();
        }

        #endregion
    }
}
