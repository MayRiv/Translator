using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public class EventArgsTranslator:EventArgs
    {
        public string[] Source;
        public string PathToLexemes;
        public string PathToSeparators;
        public string PathToSyntaxRules;
        public string PathToTableOfPriority;
        public string PathToLexemeDescription;
        public EventArgsTranslator(string[] source, string pathToLexemes, string pathToSeparators, string pathToSyntaxRules, string pathToTableOfPriority, string pathToLexemeDescription)
        {
            Source = source;
            PathToLexemes = pathToLexemes;
            PathToSeparators = pathToSeparators;
            PathToSyntaxRules = pathToSyntaxRules;
            PathToTableOfPriority = pathToTableOfPriority;
            PathToLexemeDescription = pathToLexemeDescription;
        }
    }
}
