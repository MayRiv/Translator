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
                    labelTable.Add(result[i].Remove(result[i].Length - 1), i);
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
                else ProcessOperator(poliz[current], ref operands);
                current++;
            }
        }
        private void ProcessOperator(string operation, ref Stack<string> operands)
        {
            switch (operation)
            {
                case "+":
                    {
                        double first = 0;
                        double second = 0;
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) second = idTable[arg];
                        else second = double.Parse(arg);
                        arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) first = idTable[arg];
                        else first = double.Parse(arg);
                        operands.Push((first + second).ToString());
                        break;
                    }
                case "-":
                    {
                        double first = 0;
                        double second = 0;
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) second = idTable[arg];
                        else second = double.Parse(arg);
                        arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) first = idTable[arg];
                        else first = double.Parse(arg);
                        operands.Push((first - second).ToString());
                        break;
                    }
                case "*":
                    {
                        double first = 0;
                        double second = 0;
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) second = idTable[arg];
                        else second = double.Parse(arg);
                        arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) first = idTable[arg];
                        else first = double.Parse(arg);
                        operands.Push((first * second).ToString());
                        break;
                    }
                case "/":
                    {
                        double first = 0;
                        double second = 0;
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) second = idTable[arg];
                        else second = double.Parse(arg);
                        arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) first = idTable[arg];
                        else first = double.Parse(arg);
                        operands.Push((first / second).ToString());
                        break;
                    }
                case "=":
                    {
                        double second = double.Parse(operands.Pop());
                        idTable[operands.Pop()] = second;
                        break;
                    }
                case "output":
                    {
                        string arg = operands.Pop();
                        if (idTable.ContainsKey(arg)) arg = idTable[arg].ToString();
                        view.Result = view.Result += arg + Environment.NewLine;
                        break;
                    }
                case "input":
                    {
                        string arg = operands.Pop();
                        idTable[arg] = view.GetValue();
                        break;
                    }
            }
        }
    }
}
