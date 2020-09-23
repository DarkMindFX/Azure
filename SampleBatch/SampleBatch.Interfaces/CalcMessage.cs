using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleHeavyCalc
{
    public class CalcMessage
    {
        private string inputFileName = string.Empty;

        public CalcMessage()
        {

        }

        public CalcMessage(string fileName)
        {
            this.inputFileName = fileName;
        }

        public string InputFileName
        {
            get
            {
                return this.inputFileName;
            }
            set
            {
                this.inputFileName = value;
            }
        }
    }
}
