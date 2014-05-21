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
        void Output(string message);
        //string Result { get; set; }
    }
}
