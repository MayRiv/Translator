using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml;
namespace Translator
{
    struct StateAndStack
    {
        public StateAndStack(int _state, int _stack, bool _isDefault)
        {
            state = _state;
            stack = _stack;
            isDefault = _isDefault;
        }
        public int state;
        public int stack; //0 for empty
        public bool isDefault;
    };
    class RuleForState
    {
        private int startState;
        public Dictionary<int, StateAndStack> Rules{ get; set;}
        public bool DefaultRuleExist {get; set;}
        public StateAndStack DefaultRule { get; set;}
        public string ErrorMessage {get; set;}
        public RuleForState(int state)
        {
            startState = state;
            DefaultRuleExist = false;
            Rules = new Dictionary<int,StateAndStack>();
        }
        public StateAndStack getStateAndStack(int symbol)
        {
            if (Rules.ContainsKey(symbol)) return Rules[symbol];
            if (DefaultRuleExist) return DefaultRule;
            throw new Exception(ErrorMessage);
        }
            
    }
    class SyntaxAnalyzerAutomat
    {
        private const int EXIT_STATE = -1;
        private int state;
        private int line;
        private ArrayList tableOfTransions;
        private LexemeCodeHelper lexemeCodeHelper;
        private List<LexemeLine> lexemes;
        private Stack<int> stack;
        public SyntaxAnalyzerAutomat(String pathToRules,List<LexemeLine> _lexemes, LexemeCodeHelper helper)
        {
            lexemes = _lexemes;
            lexemeCodeHelper = helper;
            tableOfTransions = new ArrayList();
            stack = new Stack<int>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToRules);
            foreach (XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                
                int numberOfState;
                if (!int.TryParse(element.GetAttribute("number"), out numberOfState))
                {
                    Console.WriteLine("Wrong format of XML");
                    throw new Exception("format exception");
                }
                var ruleForState = new RuleForState(numberOfState);

                XmlNodeList rules = element.GetElementsByTagName("rule");
                foreach (XmlElement rule in rules)
                {
                    StateAndStack returnedValue = new StateAndStack();
                    returnedValue.isDefault = false;
                    if (rule["transition"].HasAttribute("exit") && rule["transition"].GetAttribute("exit") == "true") returnedValue.state = EXIT_STATE;
                    else int.TryParse(rule["transition"].InnerText, out returnedValue.state);
                    if (!int.TryParse(rule["stack"].InnerText, out returnedValue.stack)) returnedValue.stack = 0;

                    if (rule["symbol"].HasAttribute("default"))
                    {
                        ruleForState.DefaultRuleExist = true;
                        returnedValue.isDefault = true;
                        ruleForState.DefaultRule = returnedValue;
                    }
                    else
                    {
                        int symbol = 0;
                        if (rule["symbol"].HasAttribute("id")) symbol = lexemeCodeHelper["id"];
                        else if (rule["symbol"].HasAttribute("const")) symbol = lexemeCodeHelper["const"];
                        else if (rule["symbol"].HasAttribute("logical_sign"))
                        {
                            switch (rule["symbol"].GetAttribute("logical_sign"))
                            {
                                case "less": { symbol = lexemeCodeHelper["<"]; break; }
                                case "less_equal": { symbol = lexemeCodeHelper["<="]; break; }
                                case "more": { symbol = lexemeCodeHelper[">"]; break; }
                                case "more_equal": { symbol = lexemeCodeHelper[">="]; break; }
                                default: throw new Exception("Unknown");
                            }
                        }
                        else symbol = lexemeCodeHelper[rule["symbol"].InnerText];
                        
                        ruleForState.Rules.Add(symbol, returnedValue);
                    }   
                }
                ruleForState.ErrorMessage = element["error-message"].InnerText;
                tableOfTransions.Add(ruleForState);
            }
            
        }
        public void analyze()
        {
            state = 1;

            try
            {
                for (int i = 0; i < lexemes.Count; i++)
                {
                    
                    line = lexemes[i].lineNumber;
                    var stateAndStack = ((RuleForState)tableOfTransions[state - 1]).getStateAndStack(lexemes[i].lexemeCode);  //state - 1 is because arrays starts from zero, but states from 1.
                    if (stateAndStack.stack != 0) stack.Push(stateAndStack.stack);
                    state = stateAndStack.state;
                    if (stateAndStack.isDefault) i--;
                    if (state == EXIT_STATE)
                    {
                        if (stack.Count == 0)
                        {
                            Console.WriteLine("Succes!");
                            break;
                        }
                        else state = stack.Pop();
                        // To stay on the lexem.
                    }
                }
            }

           
            catch (Exception e)
            {
                Console.WriteLine("At line " + line +  ": " + e.Message);
            }
            
        }
    }
}
