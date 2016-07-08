/************************************************************************************************************************************************************
*                                        This is a C program solution to a Design Data Co. problem:                                                			*
*                                                                                                                                                           *
*                                        write a C++ program to parse a character string and convert it into a decimal value.                               *
*                                                                                                                                                           *
*                                        Expected outcome:                                                                                                  *
*                                                                                                                                                           *
*                                        "10 1/2"   should output 10.5                                                                                      *
*                                        "10 1/16"  should output 10.0625                                                                                   *
*                                        "3/4"      should output .75*                                                                                      *
*                                                                                                                                                           *
*                                                                                                                    Author:      Keith Israel              *
*                                                                                                                    Location:    Des Moines, IA            *
*                                                                                                                    Date:          7/8/2016                *
*                                                                                                                                                           *
*                                                                                                                    ALL RIGHTS RESERVED.                   *
*************************************************************************************************************************************************************/

#include <iostream>
#include <vector>
#include <string>
#include <sstream>

using namespace std;

// Class Declaration
class math
{
    string input;
    float output=0.00;
    
    vector<string> strArgs;
    vector<string> subArgs;
    
private:
    vector<string> split(string, char);
    string trim(const std::string& str,
                 const std::string& whitespace = " \t");

public:
    bool getdata();
    void convert();
    void display();
};

//Main Function
 int main()
{
    
    math obj;

    if(obj.getdata())
    {
        obj.convert();
        obj.display();    
    }
    
    return 0;
}

bool math::getdata()
{
    //Get Input Values For input variable
    cout<<"Enter a value :";
    getline (cin, input);
    
    if(!input.empty())
        return true;
    else
        return false;
    
}

void math::convert()
{
    //trim input
    input  = trim(input);
    
    //search for white-space (multiple arguments) 
    vector<string> strArgs = split(input, ' ');
    
    //if white space, split screen
    if (!strArgs.empty())
    {
        //
            //1. split string to multiple tokens 
            //2. if more than 2 tokens, return error
            if(strArgs.size() > 2)
            {
                cout << "Invalid input entered. Please enter a valid input" << endl;
                return;
            }
            //3. if first token contains '/' return error
            std::size_t found = (strArgs[0]).find("/");
            if (found!=std::string::npos)
            {
                cout << "Invalid input entered. Please enter a valid input" << endl;
                return;
            }
            //4. convert first token to int
            int decValue = stoi(strArgs[0]);
        
            //5. split second token to fraction tokens
            vector<string> subArgs = split(strArgs[1], '/');
             //  i. divide fraction-top token with fraction-bottom token        
             //  ii.store to float no.
            output = stoi(subArgs[0]) / (float)(stoi(subArgs[1])); 
            //6. add first token with float no.
            output = (float)decValue + output;
        
            //test
            //cout << "in convert 2" << endl;
            //for(string t : strTokens)
            //cout << t << endl;
   
        return;
    }
    //test
    //cout << "in convert 3" << endl;
    
    subArgs = split(input, '/');
    output = stoi(subArgs[0]) / (float)(stoi(subArgs[1])); 
    
    return;
}

vector<string> math::split(string str, char delimiter) 
{
    vector<string> internal;
    
    stringstream ss(str); // Turn the string into a stream.
    string tok;
  
    while(getline(ss, tok, delimiter)) 
    {
        internal.push_back(tok);
    }
    
    if(internal.size() == 1)
        internal.clear();
  
    return internal;
}

string math::trim(const std::string& str,
                 const std::string& whitespace)
{
    const auto strBegin = str.find_first_not_of(whitespace);
    if (strBegin == std::string::npos)
        return ""; // no content

    const auto strEnd = str.find_last_not_of(whitespace);
    const auto strRange = strEnd - strBegin + 1;

    return str.substr(strBegin, strRange);
}

void math::display()
{
   //Show the Output
    cout <<"\nValue = " <<output << endl;
}
