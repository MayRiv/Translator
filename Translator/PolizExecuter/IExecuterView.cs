using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolizInterpretator
{
    public interface IExecuterView
    {
        double GetValue();
        string Result { get; set; }
    }
}
