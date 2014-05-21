using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzerLibrary
{
    public struct LexemeLine
    {
        public int lineNumber;
        public string lexeme;
        public int lexemeCode;
        public int? identifierIndex;
        public string value; //kostul

        public LexemeLine(int newLineNumber, string newLexeme, int newLexemeCode, int? newIdentifierIndex)
        {
            lineNumber = newLineNumber;
            lexeme = newLexeme;
            lexemeCode = newLexemeCode;
            identifierIndex = newIdentifierIndex;
            value = lexeme;
        }
        public void Display()
        {
            Console.WriteLine("{0:D4} | {1,20} | {2:D3} | {3}", lineNumber, lexeme, lexemeCode, identifierIndex);
        }
    }
}
