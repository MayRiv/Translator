using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzerLibrary
{
    public class LexemeCodeHelper
    {
        private List<string> lexemes;
        private int identifierCode;
        private int constantCode;
        public LexemeCodeHelper(List<string> l)
        {
            lexemes = l;
            identifierCode = lexemes.Count;
            constantCode = lexemes.Count + 1;
        }
        public int this[string index]
        {

            get
            {
                if (index.Equals("id")) return identifierCode;
                if (index.Equals("const")) return constantCode;
                return lexemes.IndexOf(index);
            }
        }

    }
}
