using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public interface ITranslatorView:IInput
    {
        string Source { get; }
        string Result { get; set;}
        event EventHandler<EventArgsTranslator> TryAnalyze;
    }
}
