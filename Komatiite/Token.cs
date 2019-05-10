

public class Token
{

    public Token() { }

    public Token(TokenType type, int startIndex, int endIndex)
    {
        TokenType = type;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public TokenType TokenType { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }

    public string GetSubstring(string template) {
        if(this.StartIndex < 0 || this.EndIndex >= template.Length || this.EndIndex < this.StartIndex) return "";
        else return template.Substring(this.StartIndex, this.EndIndex - this.StartIndex + 1);
    }

}