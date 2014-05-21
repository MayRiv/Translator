using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Translator
{
    public partial class Form1 : Form, ITranslatorView
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string Source
        {
            get
            {
                return sourceTextBox.Text;
            }
        }
        public string Result
        {
            get
            {
                return resultTextBox.Text;
            }
            set
            {
                resultTextBox.Invoke(new Action(() => { resultTextBox.Text = value; }));
            }
        }
        public event EventHandler<EventArgsTranslator> TryAnalyze;
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.InitialDirectory = Environment.CurrentDirectory;
            f.ShowDialog();
            var lines = File.ReadAllLines(f.FileName);
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb = sb.AppendLine(line);
            }
            sourceTextBox.Text = sb.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            char[] separator = new char[2];
            separator[0] = '\r';
            separator[1] = '\n';
            
            string[] source = Source.Split(separator);
            string[] clearedSource = new string[source.Count() / 2];
            for (int i = 0; i < clearedSource.Count(); i++)
                clearedSource[i] = source[2 * i];


            EventArgsTranslator args = new EventArgsTranslator(clearedSource, @"lexemesJeka.txt", @"separators.txt", @"syntax_states2.xml");
            TryAnalyze.Invoke(this, args);
        }

        
    }
}
