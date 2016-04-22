// RomanNumeralsKata1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <map>
#include <string>

using namespace std;    

int inputNumber = 0;
map<int, string> romanKeyValues;
int decimalValues[9] = { 1000, 500, 100, 50, 10, 9, 5, 4 , 1};
char romanValues[9] = { 'M', 'D', 'C', 'L', 'X', 'IX', 'V', 'IV' , 'I'};

string printRomanNumeral(int);
string iterate(int, char);
void inputRules(string&);
string alterString(string, char);
void replaceString(string&, string, string);
int getposition(const char *, size_t, char);

int main()
{
	while (true)
	{
		cout << "Please enter a number greater than zero: ";
		cin >> inputNumber;

		if (inputNumber > 0)
		{
			string validString = printRomanNumeral(inputNumber);
			if (!validString.empty())
				cout << "The Roman Numeral is: " << validString << endl;
			else
				cout << "Invalid Number Entered." << endl;
		}
	}
	return 0;
}

string printRomanNumeral(int inputNumber)
{
	string romanValue = "";
	int j = 0;
	for (int i : decimalValues)
	{
		int mod = inputNumber % i;
		int tmp = inputNumber / i;
		inputNumber = mod;

		if (tmp > 0)
			romanValue.append(iterate(tmp, romanValues[j]));
		j++;
	}
	inputRules(romanValue);
	return romanValue;
}

string iterate(int count, char romanNumeral)
{
	int j = 0;
	string roman = "";
	string s(1, romanNumeral);

	while (j < count)
	{
		roman.append(s);
		j++;
	}
	return roman;
}

void inputRules(string& initialValue)
{
	string alteredString = "";
	char tmp[1024] = {0};

	strncpy_s(tmp, initialValue.c_str(), initialValue.size()+1);

	for (int i=0; i<initialValue.size(); i++)
	{ 
		char c = tmp[i];
		if (c != 0)
		{
			//check if there's a sequence of characters
			bool chkSequence = false;
			for (int k = 1; k < 4; k++)
			{
				if (c == tmp[i + k])
					chkSequence = true;
				else
				{
					chkSequence = false;
					break;
				}
			}
			if (chkSequence)  
			{
				string temp = "";
				int pos = getposition(tmp, sizeof(tmp), c);   //get the position of first occurence of sequence
				int j = 0;
				if (pos > 0)
					j = pos - 1;   //the position of the character that's just before the sequence

				for (; j < pos + 5; j++)
				{
					string s(1, tmp[j]);
					temp.append(s);
				}
				alteredString = alterString(temp, tmp[pos]);
				replaceString(initialValue, temp, alteredString);
				if (initialValue.empty())
					return;
				i = pos + 4;
			}
		}
	}
}

int getposition(const char *array, size_t size, char c)
{
	for (size_t i = 0; i < size; i++)
	{
		if (array[i] == c)
			return (int)i;
	}
	return -1;
}

string alterString(string s, char C)
{
	string tmp = "";
	switch (C)
	{
		case 'I':
			if (s[0] == 'V')
				tmp = "IX";
			else
			{ 
				string s(1, s[0]);
				(tmp.append(s)).append("IV");
			}
			break;
		case 'X':
			tmp = "XL";
			break;
		case 'C':
			tmp = "CD";
			break;
		case 'M':
			break;
		default:
			break;
	}
	return tmp;
}

void replaceString(string& str, string oldStr, string newStr)
{
	if (newStr != "")
	{
		size_t index = 0;
		while (true) {
			/* Locate the substring to replace. */
			index = str.find(oldStr, index);
			if (index == std::string::npos) break;

			/* Make the replacement. */
			str.replace(index, sizeof(oldStr), newStr);

			/* Advance index forward so the next iteration doesn't pick it up as well. */
			index += sizeof(newStr);
		}
	}
	str.clear();
}