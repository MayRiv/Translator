using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolizInterpretator;
namespace Translator
{
    public interface ITranslatorView:IExecuterView
    {
        string Source { get; }
        event EventHandler<EventArgsTranslator> TryAnalyze;
    }
}
