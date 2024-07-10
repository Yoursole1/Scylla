using UnityEngine;
using System.Collections.Generic;
using System;

public class Scylla
{   
    // Tokens need a type.  Keyword, identifier, numerical, operator, ect

    public static bool run(string code)
    {
        Tokenizer t = new Tokenizer(code);
        IEnumerable<Token> tokens = t.tokenize();

        foreach(Token token in tokens)
        {
            Debug.Log(token.value);
        }
        return false;
    }

    public class Token
    {
        public string value { get; }
        public uint priority { get; }

        public Token(string value, uint priority)
        {
            this.value = value;
            this.priority = priority;
        }
    }

    public class Tokenizer
    {
        private string code;
        public Tokenizer(string code)
        {
            this.code = code;
        }

        public IEnumerable<Token> tokenize()
        {
            List<Token> tokens = new List<Token>();

            for(int i = 0; i < this.code.Length; i++)
            {
                char curr = this.code[i];

                char next = (i == this.code.Length - 1) ? ' ' : this.code[i + 1];
                Token t = TokenFetcher.get(curr, next);
                if(t == null)
                {
                    continue;
                }

                tokens.Add(t);
            }

            if(TokenFetcher.fetch != "")
            {
                //error tokenizing since there are charactors left over that were not parsed properly
            }
            return tokens;
        }

    }

    
    private class TokenFetcher
    {
        // ()+-/*{}[],= 
        // while number sequence
        public static string fetch = "";
        private static bool ident = false;

        public static Token get(char curr, char next)
        {

            if (ident)
            {
                if(next == '=')
                {
                    ident = false;
                    fetch = "";
                    return new Token("ident", 0); // find way to actually get an ident token without instantiation
                }
            }

            if (curr == ' ')
            {
                return null;
            }

            fetch += curr;

            bool fetchInTokens = false;
            Token found = null;
            foreach (Token t in TokenFetcher.tokens)
            {
                if(fetch == t.value)
                {
                    fetchInTokens = true;
                    found = t;
                    break;
                }
            }

            if (fetchInTokens)
            {
                string nextFetch = fetch + next;
                bool nextFetchInTokens = false;
                foreach (Token t in TokenFetcher.tokens)
                {
                    if (nextFetch == t.value)
                    {
                        nextFetchInTokens = true;
                    }
                }

                if (!nextFetchInTokens)
                {

                    if (found.value == "number" || found.value == "sequence") // data type
                    {
                        ident = true;
                    }

                    fetch = "";
                    return found;
                }
            }
            return null;
        }
        
        private static List<Token> tokens = new List<Token> {
            new Token("(", 0),
            new Token(")", 0),
            new Token("+", 0),
            new Token("-", 0),
            new Token("/", 0),
            new Token("*", 0),
            new Token("{", 0),
            new Token("}", 0),
            new Token("[", 0),
            new Token("]", 0),
            new Token(",", 0),
            new Token("=", 0),

            new Token("while", 0),
            new Token("number", 0),
            new Token("sequence", 0),

            new Token("ident", 0)
        }; 
    }
}

