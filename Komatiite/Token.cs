

using System.Text;

public class Token
{
    public Token(TokenType type)
    {
        TokenType = type;
        StartPosition = CharacterPosition.Empty;
        EndPosition = CharacterPosition.Empty;
    }
    public Token(TokenType type, CharacterPosition startPosition)
    {
        TokenType = type;
        StartPosition = startPosition;
        EndPosition = startPosition;
    }
    public Token(TokenType type, CharacterPosition startPosition, CharacterPosition endPosition)
    {
        TokenType = type;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }

    public TokenType TokenType { get; set; }
    public CharacterPosition StartPosition { get; set; }
    public CharacterPosition EndPosition { get; set; }
    public int Length
    {
        get
        {
            return EndPosition.Index - StartPosition.Index;
        }
    }

}