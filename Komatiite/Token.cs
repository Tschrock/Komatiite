

using System.Text;

public class Token
{
    public Token(TokenType type, CharacterPosition startPosition)
    {
        TokenType = type;
        StartPosition = startPosition;
    }

    public TokenType TokenType { get; set; }
    public CharacterPosition StartPosition { get; set; }

}