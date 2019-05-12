using System;
using System.Web;

namespace Komatiite.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer("Hello {{- Person.FirstName }}! {{ thing[0] }} {{ thing[\"prop\"] }} {{ -3 }} {{ 0.321 }} {{ \"string asdf\" }} {{ 'asdf\"fdas' }} {{ \"asdf\\\"asdf\" }} {{ \"\" }}");
            foreach (var token in lexer)
            {
                Console.WriteLine("Token: {0}  Start: {1}  End: {2}", token.TokenType, token.StartPosition.Index, token.EndPosition.Index);
            }
        }
    }
}
