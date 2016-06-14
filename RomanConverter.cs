using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanConverter
{
    public class RomanConverter
    {
        Dictionary<int, string> romanTable = null;

        public RomanConverter()
        {
            romanTable = new Dictionary<int, string>();
            romanTable.Add(1000, "M");
            romanTable.Add(500, "D");
            romanTable.Add(100, "C");
            romanTable.Add(50, "L");
            romanTable.Add(10, "X");
            romanTable.Add(5, "V");
            romanTable.Add(1, "I");
        }

        public string convert(int inputNumber)
        {
            return romanTable[inputNumber];
        }
    }
}
