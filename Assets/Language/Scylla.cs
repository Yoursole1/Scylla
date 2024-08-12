using UnityEngine;
using System.Collections.Generic;
using System;

public class Scylla
{   


    public static bool run(string code)
    {
        Tokenizer t = new Tokenizer(code);
        IEnumerable<Token> tokens = t.tokenize();

        foreach(Token token in tokens)
        {
            Debug.Log(token.value + "-" + token.type);
        }
        return false;
    }

    public class ScyllaError : Exception
    {
        public ScyllaError(string message) : base(message) {}
    }

    public enum TokenType
    {
        KEYWORD, NUM, SEQ, IDENT, OPERATOR
    }

    public class Token
    {
        public string value { get; }
        public uint priority { get; }
        public TokenType type { get; }

        public Token(string value, uint priority, TokenType type)
        {
            this.value = value;
            this.priority = priority;
            this.type = type;
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
            TokenFetcher fetcher = new TokenFetcher();

            for(int i = 0; i < this.code.Length; i++)
            {
                char curr = this.code[i];

                char next = (i == this.code.Length - 1) ? ' ' : this.code[i + 1];
                Token t = fetcher.get(curr, next);

                if(t != null)
                {
                    tokens.Add(t);
                }
            }

            if(fetcher.fetch != "")
            {
                //error tokenizing since there are charactors left over that were not parsed properly
            }
            return tokens;
        }

    }

    
    private class TokenFetcher
    {
        private enum TokenFetcherState
        {
            IDENT_PRESPACE, IDENT_POSTSPACE, NUM, SEQ, NORM
        }
        // ()+-/*{}[],= 
        // while number sequence
        public string fetch { get; set; }
        private TokenFetcherState state;
        private TokenType? typeIfVarBuilding; // null unless currently initializing a variable, in which case it is the type of var


        public TokenFetcher()
        {
            this.fetch = "";
            this.state = TokenFetcherState.NORM;
        }

        public Token get(char curr, char next)
        {
            
            this.fetch += curr;

            switch (this.state)
            {
                case TokenFetcherState.NORM:
                    {
                        Token t = getToken(this.fetch);
                        if(this.fetch == "number") 
                        {
                            this.state = TokenFetcherState.IDENT_PRESPACE;
                            this.typeIfVarBuilding = TokenType.NUM;
                        }else if(this.fetch == "sequence")
                        {
                            this.state = TokenFetcherState.IDENT_PRESPACE;
                            this.typeIfVarBuilding = TokenType.SEQ;
                        }

                        if(this.fetch == "=")
                        {
                            switch (this.typeIfVarBuilding)
                            {
                                case TokenType.NUM:
                                    {
                                        this.state = TokenFetcherState.NUM;
                                        break;
                                    }
                                case TokenType.SEQ:
                                    {
                                        this.state = TokenFetcherState.SEQ;
                                        break;
                                    }
                            }
                            this.typeIfVarBuilding = null;
                        }

                        if(t != null)
                        {
                            this.fetch = "";
                        }

                        return t;
                    }
                    
                case TokenFetcherState.IDENT_PRESPACE:
                    {
                        if(curr == ' ')
                        {
                            this.state = TokenFetcherState.IDENT_POSTSPACE;
                        }
                        else
                        {
                            throw new ScyllaError("Expected \" \" after type keyword");
                        }
                        return null;
                    }
                case TokenFetcherState.IDENT_POSTSPACE:
                    {
                        string forbiddenChar = "!@#$%^&*()_+-{}[]:\";'<>?,./' ";

                        if(forbiddenChar.Contains(curr)){
                            throw new ScyllaError("Invalid charactor in variable name: " + curr);
                        }

                        if(next == '=')
                        {
                            this.state = TokenFetcherState.NORM;

                            Token t = new Token(this.fetch, 0, TokenType.IDENT);
                            this.fetch = "";
                            return t;
                        }
                        break;
                    }
                    
                case TokenFetcherState.NUM:
                    {
                        if(next == '\n')
                        {
                            Token t = new Token(this.fetch, 0, TokenType.NUM);
                            this.fetch = "";
                            return t;
                        }
                        
                        break;
                    }
                case TokenFetcherState.SEQ:
                    {
                        if (next == '\n')
                        {
                            Token t = new Token(this.fetch, 0, TokenType.SEQ);
                            this.fetch = "";
                            return t;
                        }

                        break;
                    }
            }

            return null;
        }

        private static Token getToken(string name)
        {
            foreach(Token t in tokens)
            {
                if(t.value == name)
                {
                    return t;
                }
            }
            return null;
        }

        private static List<Token> tokens = new List<Token> {
            new Token("(", 0, TokenType.KEYWORD),
            new Token(")", 0, TokenType.KEYWORD),
            new Token("+", 0, TokenType.OPERATOR),
            new Token("-", 0, TokenType.OPERATOR),
            new Token("/", 0, TokenType.OPERATOR),
            new Token("*", 0, TokenType.OPERATOR),
            new Token("{", 0, TokenType.KEYWORD),
            new Token("}", 0, TokenType.KEYWORD),
            new Token("[", 0, TokenType.KEYWORD),
            new Token("]", 0, TokenType.KEYWORD),
            new Token(",", 0, TokenType.KEYWORD),
            new Token("=", 0, TokenType.KEYWORD),
            new Token("\n", 0, TokenType.KEYWORD),

            new Token("while", 0, TokenType.KEYWORD),
            new Token("number", 0, TokenType.KEYWORD),
            new Token("sequence", 0, TokenType.KEYWORD),

            
        }; 
    }
}

