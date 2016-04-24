/******************************************************************************************************************************************************
*							This is the solution to the Roman Numerals problem presented on the	url link:											  *
* 																																					  *
*							http://agilekatas.co.uk/katas/RomanNumerals-Kata																		  *
*																																					  *
*							The solution does not implement OOP design. It's a 'flow' implementation.												  *
*																																					  *
*																			Author:      Keith Israel                                                 *
*                                                                         	Location:    Des Moines, IA  USA                                          *
*                                                                         	Year:        April 2016                                                   *
*                                                                                                                                                     *
*                                                                                                                   All Rights Reserved.              *
*																																					  *
*																																			  		  *
*******************************************************************************************************************************************************/

//#include "stdafx.h"

#include <iostream>
#include <map>
#include <string>

using namespace std;

int inputNumber = 0;
map<int, string> romanKeyValues;
int decimalValues[7] = { 1000, 500, 100, 50, 10, 5, 1};
char romanValues[7] = { 'M', 'D', 'C', 'L', 'X', 'V', 'I'};

string printRomanNumeral(int);
string iterate(int, char);
void inputRules(string&);
void alterString(string&, string&, char);
void replaceString(string&, string, string, int&);
int getposition(const char *, size_t, char);

int main()
{
	while (true)
	{
		cout << "Please enter a number between 1 and 3999: ";
		cin >> inputNumber;

		if (inputNumber > 0)
		{
			string validString = printRomanNumeral(inputNumber);
			if (!validString.empty())
				cout << "The Roman Numeral is: " << validString << endl << endl;
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
		int tmp = inputNumber / i;
		int mod = inputNumber % i;
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
	string alteredString = "", tempString = "";
	char tmp[1024] = { 0 };

	for (int i=0; i<initialValue.size(); i++)
	{
		strncpy_s(tmp, initialValue.c_str(), initialValue.size() + 1);
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

				for (; j < pos + 4; j++)
				{
					string s(1, tmp[j]);
					temp.append(s);
				}
				alterString(alteredString, temp, tmp[pos]);
				replaceString(initialValue, temp, alteredString, pos);
				if (initialValue.empty())
					return;
				i = pos;
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

void alterString(string& newStr, string& oldStr, char C)
{
	char sumOfRomans[10], chk='0';
	int tmpNo = inputNumber;
	int i = 0, k=0, pos=-1, count=0, p=0; // the array index
	int a[4] = {0}, a2[4] = {0};
	string str(1, C);
	newStr.clear();

	while (tmpNo) {
		a[i] = tmpNo % 10;
		tmpNo /= 10;
		i++;
	}
	if (a[i] == 0)
	{
		while (a[i] == 0)
		{
			i--;
		}
		i++;
	}
	k = i;
	switch (C)
	{
	case 'I':
		while (a[k] != 4)
		{
			if (k == 0) break;
			k--;
		}
		if (a[k] == 4 /*&& a[k+1] == 0*/)
			oldStr.erase(0, 1);
	case 'X':
		if (C == 'X')
		{
			strncpy_s(sumOfRomans, oldStr.c_str(), oldStr.length());
			for (p = 0; p < oldStr.size(); p++)
			{
				chk = sumOfRomans[p];
				pos = getposition(romanValues, sizeof(romanValues), chk);
				count += decimalValues[pos];
			}
			pos = 0;
			while (count) {
				a2[pos] = count % 10;
				count /= 10;
				pos++;
			}
		}
	case 'C':
		newStr.append(str);

		if (pos>-1 && a2[pos-1]!=9 && oldStr[1] == C)
		{
			if (oldStr[0]==C)
				newStr += romanValues[pos + 1];
			else
			{
				if (oldStr[0] == 'D' || oldStr[0] == 'V')
					newStr += romanValues[pos - 2];
				else
					newStr += romanValues[pos - 1];
			}
		}
		else
		{
			pos = getposition(romanValues, sizeof(romanValues), C);
			if (a[i - 2] == 0)
			{
				if (i == 3)
				{
					newStr.clear();
					newStr.append(str);
					if(a[0]==9)
						newStr += romanValues[pos - 2];
					else
						newStr += romanValues[pos - 1];
				}
				else
					newStr += romanValues[pos - 2];
			}
			else
			{
				if (a[0] == 4)
					newStr += romanValues[pos - 1];
				else
					newStr += romanValues[pos - 2];
			}
		}
		break;
	case 'M':
		break;
	default:
		break;
	}
}

void replaceString(string& str, string oldStr, string newStr, int& position)
{
	if (newStr != "")
	{
		size_t index = 0;
		/* Locate the substring to replace. */
		index = str.find((oldStr.c_str()), index);
		if (index == std::string::npos)
		{
			str = "";
			return;
		}

		position = index + 1;
		/* Make the replacement. */
		str.replace(index, oldStr.length(), newStr);
	}
	else
		str = newStr;
}