using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanConverter
{
    public class RomanConverter
    {
        Dictionary<int, string> romanTable;

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
            if (inputNumber < 1 || inputNumber > 3999)
                throw new ArgumentException("Number out of range: 1-3999",
                        new System.Collections.Generic.KeyNotFoundException());
            try
            {
                return romanTable[inputNumber];
            }
            catch (Exception ex)
            {
                String romanValue = String.Empty;
                int itNo = 0, result = inputNumber - itNo;


                while (result != 0)
                {
                    itNo = iterateRomanTable(result);
                    romanValue += romanTable[itNo];
                    result = inputNumber - itNo;
                    inputNumber = result;
                }
                return inputRules(romanValue);
            }
        }

        int iterateRomanTable(int no)
        {
            int divNo = 0;
            foreach (var keyValue in romanTable)
            {
                if (no >= keyValue.Key)
                    return keyValue.Key;
            }
            return divNo;
        }

        string inputRules(string romanValue)
        {
            char[] tmp = romanValue.ToCharArray();
            int charLen = romanValue.Length;
            string newStr = string.Empty, tempStr = romanValue;

            while (charLen != 0)
            {
                string n = checkSequence(tmp, romanValue.Length - charLen, romanValue);
                if (!n.Equals(newStr) && !n.Equals(romanValue))
                {
                    string oldValue = romanValue.Substring((romanValue.Length - charLen) - 1, 5);
                    tempStr = tempStr.Replace(oldValue, n);
                }
                else if (!n.Equals(newStr))
                    newStr += n;
                charLen--;
            }

            return tempStr;
        }

        string checkSequence(char[] tempStr, int currentIndex, string oldRomanValue)
        {
            string newStr = string.Empty;
            int i = currentIndex;
            try
            {

                if (oldRomanValue.Length == 3)
                    return oldRomanValue;
                else if (tempStr[i] == tempStr[i + 1] && tempStr[i] == tempStr[i + 2] &&
                    tempStr[i] == tempStr[i + 3])
                {
                    switch (tempStr[i])
                    {
                        case 'I':
                            return newStr = "IX";
                        case 'V':
                        case 'X':
                        case 'C':
                        case 'D':
                        default:
                            List<int> keys = romanTable.Keys.ToList();
                            List<string> values = romanTable.Values.ToList();
                            newStr += tempStr[i];

                            string c = (tempStr[i - 1]).ToString();
                            int j = 0;
                            foreach (var val in romanTable.Values.ToList())
                            {
                                if (j != 0 && val.Equals(c))
                                {
                                    int key = keys.ElementAt(j - 1);
                                    newStr += romanTable[key];
                                }
                                j++;
                            }
                            return newStr;
                    }
                }
            }
            catch (Exception ex)
            { }
            return oldRomanValue;
        }
    }
}
