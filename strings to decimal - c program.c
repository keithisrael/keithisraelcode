#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

char** str_split(char* , const char );

/* Maximum name size + 1. */
#define MAX_NAME_SZ 256

int main(int argC, char *argV[]) {
    
    char** tokens, **fracTokens, *c;
    const char *specChar = "/";
    int fracBool = 0, decValue = 0, count = 0;
    float **arr = (float**)malloc(sizeof(float)),fraction;
    
    /* Allocate memory and check if okay. */
    char *value = (char*)malloc (MAX_NAME_SZ);
    if (value == NULL) {
        printf ("No memory\n");
        return 1;
    }

    /* Ask user for the number. */
    printf("Please enter a value: \n");

    /* Get the number, with size limit. */
    fgets (value, MAX_NAME_SZ, stdin);

    /* Remove trailing newline, if there. */
    if ((strlen(value)>0) && (value[strlen (value) - 1] == '\n'))
        value[strlen (value) - 1] = '\0';
    
    tokens = str_split(value, ' ');

    if (tokens)
    {
        int i;
        for (i = 0; *(tokens + i); i++)
        {
            //printf("%s\n", *(tokens + i));
            
            //check if special char exist
            c = (char*)malloc((strlen(*(tokens + i)) + 1) * sizeof(char));
            strcpy(c, (*(tokens + i)));
            
            while (*c)
            {
                if (strchr(specChar, *c))
                {
                    //printf("%c is in \"%s\"\n", *c, *(tokens + i));
                    fracBool = 1;
                }

                c++;
            }
            
            if(fracBool)
            {
                fracTokens = str_split((*(tokens + i)), '/');
                
                if (fracTokens)
                {
                    int j, top=0, bottom=0;
                    
                    top = atoi(*(fracTokens + 0));
                    bottom = atoi(*(fracTokens + 1)); 
                    
                    fraction = top / (float)bottom;
                    arr[count] = (float*)malloc(sizeof(float));
                    *arr[count] = fraction;
                    //printf("%f", *arr[count]);//fraction);
                    count++;
                    
                    //printf("\n");
                    free(fracTokens);
                }
                
                fracBool = 0;
            }
            else
            {
                decValue = atoi(*(tokens + i));
                arr[count] = (float*)malloc(sizeof(float));
                *arr[count] = ((float)decValue);  
                //printf("%f\n", *arr[count]);//decValue);         
                count++;     
            }
            
            free(*(tokens + i));
        }
        
        fraction = 0;
        while(count>0)
        {
            fraction += *arr[count-1];
            count--;
        }
        printf("\nValue = %f\n", fraction);
        
        free(tokens);
    }
    /* Free memory and exit. */
    free (value);
    return 0;
}


char** str_split(char* a_str, const char a_delim)
{
    char** result    = 0;
    size_t count     = 0;
    char* tmp        = a_str;
    char* last_comma = 0;
    char delim[2];
    delim[0] = a_delim;
    delim[1] = 0;

    /* Count how many elements will be extracted. */
    while (*tmp)
    {
        if (a_delim == *tmp)
        {
            count++;
            last_comma = tmp;
        }
        tmp++;
    }

    /* Add space for trailing token. */
    count += last_comma < (a_str + strlen(a_str) - 1);

    /* Add space for terminating null string so caller
       knows where the list of returned strings ends. */
    count++;

    result = (char**)malloc(sizeof(char*) * count);

    if (result)
    {
        size_t idx  = 0;
        char* token = strtok(a_str, delim);

        while (token)
        {
            assert(idx < count);
            *(result + idx++) = strdup(token);
            token = strtok(0, delim);
        }
        assert(idx == count - 1);
        *(result + idx) = 0;
    }

    return result;
}