using System;
using System.Web;

namespace Komatiite.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer("Hello {{- Person.FirstName }}!");
            foreach (var token in lexer)
            {
                Console.WriteLine("Token: {0}  Start: {1}  End: {2}  Text: \"{3}\"", token.TokenType, token.StartIndex, token.EndIndex, HttpUtility.JavaScriptStringEncode(token.GetSubstring(lexer.Template)));
            }
        }
    }
}
