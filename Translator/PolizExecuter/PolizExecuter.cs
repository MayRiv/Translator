using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolizInterpretator
{
    public class PolizExecuter
    {
        private IExecuterView view;
        private string[] poliz;
        private int current = 0;
        private Dictionary<string, double> idTable;
        private Dictionary<string, int> labelTable;
        private Dictionary<string, double> constTable;
        public PolizExecuter(string poliz, List<string> ids, List<double> consts, IExecuterView view)
        {
            this.view = view;
            idTable = new Dictionary<string, double>();
            constTable = new Dictionary<string, double>();
            foreach(double c in consts)
                constTable.Add(c.ToString(),c);
            this.poliz = Prepare(poliz);
            
            foreach (string id in ids)
                idTable.Add(id, 0);

        }
        private string[] Prepare(string poliz)
        {
            string[] result = poliz.Split(' ');
            result = ProcessLabels(result);
            ProcessCycleCells(result);
            return result;
        }
        private string[] ProcessLabels(string[] result)
        {
            
            labelTable = new Dictionary<string, int>();
            int emptyCount = 0;
            for (int j = result.Count() - 1; j > 0 && result[j].Length == 0; j--)
                emptyCount++;

            result = result.Take(result.Count() - emptyCount).ToArray();
            int sciped = 0;
            for (int i = 0; i < result.Count() - sciped; i++)
            {

                if (result[i].Last() == ':')
                {
                    labelTable.Add(result[i].Remove(result[i].Length - 1), i - 1);
                    for (int j = i; j < result.Count() - 1; j++)
                        result[j] = result[j + 1];
                    sciped++;
                    i--;
                }
            }


            return result.Take(result.Count() - sciped).ToArray();
        }
        private void ProcessCycleCells(string[] poliz)
        {
            foreach (string element in poliz)
            {
                if (element[0] == '_' && element[1] == '$')
                    if (!idTable.ContainsKey(element)) idTable.Add(element, 0);
                double value;
                if (double.TryParse(element, out value))
                    if (!constTable.ContainsKey(element)) constTable.Add(element,value);
            }
        }



        public void Execute()
        {
            Stack<string> operands = new Stack<string>();
            while (current < poliz.Count())
            {
                if (constTable.ContainsKey(poliz[current]))
                    operands.Push(poliz[current]);
                else if (idTable.ContainsKey(poliz[current]))
                    operands.Push(poliz[current]);
                else if (labelTable.ContainsKey(poliz[current]))
                    operands.Push(poliz[current]); 
                else ProcessOperator(poliz[current], ref operands);
                current++;
            }
        }
        private void ProcessOperator(string operation, ref Stack<string> operands)
        {
            double first;
            double second;
            switch (operation)
            {
                case "+":
                    {
                        
                        prepareOperands(ref operands,out first,out second);
                        operands.Push((first + second).ToString());
                        break;
                    }
                case "-":
                    {
                        
                        prepareOperands(ref operands, out first, out second);
                        operands.Push((first - second).ToString());
                        break;
                    }
                case "*":
                    {
                       
                        prepareOperands(ref operands, out first, out second);
                        operands.Push((first * second).ToString());
                        break;
                    }
                case "/":
                    {
                        prepareOperands(ref operands, out first, out second);
                        operands.Push((first / second).ToString());
                        break;
                    }
                case "=":
                    {
                        double arg = double.Parse(operands.Pop());
                        idTable[operands.Pop()] = arg;
                        break;
                    }
                case "output":
                    {
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) arg = idTable[arg].ToString();
                        //view.Result = view.Result += arg + Environment.NewLine;
                        view.Output(arg);
                        break;
                    }
                case "input":
                    {
                        string arg = operands.Pop();
                        idTable[arg] = view.GetValue();
                        break;
                    }
                case ">":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first > second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case "<":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first < second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case "<=":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first <= second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case ">=":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first >= second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case "==":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first == second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case "!=":
                    {
                        prepareOperands(ref operands, out first, out second);
                        if (first != second) operands.Push("1");
                        else operands.Push("0");
                        break;
                    }
                case "UPL":
                    {
                        prepareOperands(ref operands, out first, out second);
                        string secondString = second.ToString();
                        //secondString = secondString.Remove(secondString.Length - 3);
                        if (first == 0) current = int.Parse(secondString);                       
                        break;
                    }
                case "BP":
                    {
                        current = labelTable[operands.Pop()];
                        break;
                    }
            }
        }
        private void prepareOperands(ref Stack<string> operands, out double first, out double second)
        {
            first = 0;
            second = 0;
            string arg = operands.Pop();
            if (idTable.ContainsKey(arg)) second = idTable[arg];
            else if (labelTable.ContainsKey(arg)) second = labelTable[arg];
            else second = double.Parse(arg);
            arg = operands.Pop();
            if (idTable.ContainsKey(arg)) first = idTable[arg];
            else if (labelTable.ContainsKey(arg)) first = labelTable[arg];
            else first = double.Parse(arg);
        }
    }
}
