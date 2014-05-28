using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolizInterpretator;
using LexicalAnalyzerLibrary;
namespace Translator
{
    public interface ITranslatorView:IExecuterView, ILexicalOutputer
    {
        string Source { get; }
        event EventHandler<EventArgsTranslator> TryAnalyze;
    }
}
