
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

// Lava has 3 basic tag syntaxes:
// - {{ Variables }}
// - {% Tags %}
// - {[ Shortcodes ]}
//
// But if you look at the DotLiquid source there's actually 2 more hidden ones,
// which are only usable if there's no other text:
// - {{{ Shorthand Literals }}}
// - {# Shorthand Comments #}
//
// We also need special handling for long-form literals/comments, since they may
//  contain partial tags inside them which would confuse the lexer:
// - {% comment %} Something {% endcomment %}
// - {% literal %} Something {% literal %}
// - {% raw %} Something {% raw %}
//
// Lava also has a "-" modifier for trimming whitespace around tags:
// - {{- Variable -}}
// - {%- Tag -%}
// - {[- Shortcode -]}
// - etc


public class Lexer : IEnumerable<Token>, IEnumerator<Token>
{

    #region Constants

    static readonly char[] START_VARIABLE = new char[] { '{', '{' };

    static readonly char[] END_VARIABLE = new char[] { '}', '}' };

    static readonly char[] START_TAG = new char[] { '{', '%' };

    static readonly char[] END_TAG = new char[] { '%', '}' };

    static readonly char[] START_SHORTCODE = new char[] { '{', '[' };

    static readonly char[] END_SHORTCODE = new char[] { ']', '}' };

    static readonly char[] START_SHORTHAND_LITERAL = new char[] { '{', '{', '{' };

    static readonly char[] END_SHORTHAND_LITERAL = new char[] { '}', '}', '}' };

    static readonly char[] START_SHORTHAND_COMMENT = new char[] { '{', '#' };

    static readonly char[] END_SHORTHAND_COMMENT = new char[] { '#', '}' };

    #endregion


    #region Fields

    private ILavaReader reader;

    private LexerMode lexerMode;

    private Token currentToken;

    private Queue<Token> nextTokens = new Queue<Token>();

    #endregion


    public Lexer(string input) : this(new LavaStringReader(input)) { }

    public Lexer(Stream input) : this(new LavaStreamReader(input)) { }

    public Lexer(ILavaReader lavaReader) => this.reader = lavaReader;


    #region IEnumerable implimentation

    public IEnumerator<Token> GetEnumerator()
    {
        this.Reset();
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion


    #region IEnumerator implimentation

    public Token Current => currentToken;

    object IEnumerator.Current => currentToken;

    public void Reset()
    {
        this.currentToken = null;
        this.nextTokens.Clear();
    }

    public bool MoveNext()
    {
        return ReadNextToken();
    }

    #endregion


    private void ReadNextToken_Raw()
    {

        // If there's a token/eof at the current position, return that
        if (TryReadLavaStartOrEOF(0)) return;

        // Otherwise, start a RAW_TEXT token
        var rawToken = AddTokenForOffset(TokenType.RAW_TEXT, 0, 0);

        // and read until we find one
        do reader.NextCharacter();
        while (!TryReadLavaStartOrEOF(1));

        // Make sure to close the RAW_TEXT token
        rawToken.Length = this.currentIndex - rawToken.StartIndex;

    }

    private bool TryReadLavaStartOrEOF(int offset)
    {

        int c = reader.PeekCharacter(offset);

        if (IsEOF(c))
        {
            AddTokenForOffset(TokenType.EOF, offset, 0);
            return true;
        }
        else if (c == '{')
        {
            int c2 = reader.PeekCharacter(offset + 1);
            if (c2 == '{')
            {
                int c3 = reader.PeekCharacter(offset + 2);
                if (c3 == '{')
                {

                    lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
                    AddTokenForOffset(TokenType.LAVA_SHORTHAND_LITERAL_ENTER, offset, 3);

                    int c4 = reader.PeekCharacter(offset + 3);
                    if (c4 == '-')
                    {
                        AddTokenForOffset(TokenType.LAVA_TRIM_WHITESPACE_FLAG, offset + 3, 1);
                    }

                    return true;

                }
                else
                {

                    lexerMode = LexerMode.LAVA_VARIABLE;
                    AddTokenForOffset(TokenType.LAVA_VARIABLE_ENTER, offset, 2);

                    int c4 = reader.PeekCharacter(offset + 2);
                    if (c4 == '-')
                    {
                        AddTokenForOffset(TokenType.LAVA_TRIM_WHITESPACE_FLAG, offset + 2, 1);
                    }

                    return true;

                }


            }
            else if (c2 == '%')
            {

                lexerMode = LexerMode.LAVA_TAG;
                AddTokenForOffset(TokenType.LAVA_TAG_ENTER, offset, 2);

                int c4 = reader.PeekCharacter(offset + 2);
                if (c4 == '-')
                {
                    AddTokenForOffset(TokenType.LAVA_TRIM_WHITESPACE_FLAG, offset + 2, 1);
                }

                return true;


            }
            else if (c2 == '[')
            {

                lexerMode = LexerMode.LAVA_SHORTCODE;
                AddTokenForOffset(TokenType.LAVA_SHORTCODE_ENTER, offset, 2);

                int c4 = reader.PeekCharacter(offset + 2);
                if (c4 == '-')
                {
                    AddTokenForOffset(TokenType.LAVA_TRIM_WHITESPACE_FLAG, offset + 2, 1);
                }

                return true;


            }
            else if (c2 == '#')
            {

                lexerMode = LexerMode.LAVA_SHORTHAND_COMMENT;
                AddTokenForOffset(TokenType.LAVA_SHORTHAND_COMMENT_ENTER, offset, 2);

                int c4 = reader.PeekCharacter(offset + 2);
                if (c4 == '-')
                {
                    AddTokenForOffset(TokenType.LAVA_TRIM_WHITESPACE_FLAG, offset + 2, 1);
                }

                return true;


            }
        }
        return false;
    }

    private void MoveNext_Raw()
    {
        // If there's a token/eof at the current position, return that
        if (TryReadLavaStartOrEOF()) return;

        // Otherwise, start a RAW_TEXT token
        var rawToken = AddTokenFromCurrent(TokenType.RAW_TEXT, 0);

        // and read until we find one
        do this.currentIndex++;
        while (!TryReadLavaStartOrEOF());

        // Make sure to close the RAW_TEXT token
        rawToken.Length = this.currentIndex - rawToken.StartIndex;

    }

    private bool TryReadLavaStartOrEOF()
    {
        if (IsEOF(c))
        {
            AddTokenFromCurrent(TokenType.EOF, 0);
            return true;
        }
        else if (TryMatch("{{{"))
        {
            lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
            AddTokenFromCurrent(TokenType.LAVA_SHORTHAND_LITERAL_ENTER, 3);

            int peekIndex = currentIndex + 3;
            if (CharacterAt(peekIndex) == '-') AddToken(new Token(TokenType.LAVA_TRIM_WHITESPACE_FLAG, peekIndex, 1));

            return true;
        }
        else if (TryMatch("{{"))
        {
            lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
            AddTokenFromCurrent(TokenType.LAVA_TAG_ENTER, 2);
            return true;
        }
        else if (TryMatch("{%"))
        {
            lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
            AddTokenFromCurrent(TokenType.LAVA_SHORTHAND_LITERAL_ENTER, 2);
            return true;
        }
        else if (TryMatch("{["))
        {
            lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
            AddTokenFromCurrent(TokenType.LAVA_SHORTCODE_ENTER, 2);
            return true;
        }
        else if (TryMatch("{#"))
        {
            lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;
            AddTokenFromCurrent(TokenType.LAVA_SHORTHAND_COMMENT_ENTER, 2);
            return true;
        }

        return false;
    }

    private int CharacterAt(int index)
    {
        return index >= 0 && index < Template.Length ? Template[index] : -1;
    }

    private bool TryMatch(string text)
    {
        return TryMatch(this.currentIndex, text);
    }

    private bool TryMatch(char[] text)
    {
        return TryMatch(this.currentIndex, text);
    }

    private bool TryMatch(int startIndex, string text)
    {
        if (startIndex < 0 || startIndex + text.Length >= this.Template.Length) return false;
        for (int i = 0; i < text.Length; startIndex++, i++)
        {
            if (this.Template[startIndex] != text[i]) return false;
        }
        return true;
    }

    private bool TryMatch(int startIndex, char[] text)
    {
        if (startIndex + text.Length >= this.Template.Length) return false;
        for (int i = 0; i < text.Length; startIndex++, i++)
        {
            if (this.Template[startIndex] != text[i]) return false;
        }
        return true;
    }

    private void AddToken(Token newToken)
    {
        if (this.currentToken == null) this.currentToken = newToken;
        else this.nextTokens.Enqueue(newToken);
    }
    private Token AddTokenFromCurrent(TokenType tokenType, int length)
    {
        if (length > 0) length--;
        var token = new Token(tokenType, this.currentIndex, this.currentIndex + length);
        this.AddToken(token);
        return token;
    }

    private bool ReadString(int c)
    {

        var startIndex = this.currentIndex;
        var foundEnd = false;

        while (++this.currentIndex < this.Template.Length)
        {
            char c2 = this.Template[this.currentIndex];
            if (c2 == '\\') this.currentIndex++;
            if (c2 == c)
            {
                foundEnd = true;
                break;
            }
        }
        var endIndex = this.currentIndex;

        if (foundEnd)
        {
            AddToken(new Token(TokenType.STRING_START, startIndex, startIndex));
            AddToken(new Token(TokenType.RAW_TEXT, startIndex + 1, endIndex - 1));
            AddToken(new Token(TokenType.STRING_END, endIndex, endIndex));
        }
        else
        {
            AddToken(new Token(TokenType.STRING_START, startIndex, startIndex));
            AddToken(new Token(TokenType.RAW_TEXT, startIndex + 1, endIndex));
            AddToken(new Token(TokenType.EOF, -1, -1));
        }
        return true;
    }

    private void MoveNext_LavaVariable()
    {
        SkipWhitespace();

        int c = CharacterAt(this.currentIndex);

        if (TryMatch("-}}"))
        {
            lexerMode = LexerMode.RAW;
            AddTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);
            AddTokenFromCurrent(TokenType.LAVA_VARIABLE_EXIT, 2);
        }
        if (TryMatch("}}"))
        {
            lexerMode = LexerMode.RAW;
            AddTokenFromCurrent(TokenType.LAVA_VARIABLE_EXIT, 2);
        }
        else if (c == '[') AddTokenFromCurrent(TokenType.LEFT_SQUARE_BRACKET, 1);
        else if (c == ']') AddTokenFromCurrent(TokenType.RIGHT_SQUARE_BRACKET, 1);
        else if (c == '(') AddTokenFromCurrent(TokenType.LEFT_PARENTHESES, 1);
        else if (c == ')') AddTokenFromCurrent(TokenType.RIGHT_PARENTHESES, 1);
        else if (TryMatch("!=")) AddTokenFromCurrent(TokenType.NOT_EQUAL, 2);
        else if (TryMatch("==")) AddTokenFromCurrent(TokenType.EQUALS, 2);
        else if (c == '=') AddTokenFromCurrent(TokenType.ASSIGNMENT, 1);
        else if (TryMatch("<=")) AddTokenFromCurrent(TokenType.LESS_THAN_OR_EQUAL, 2);
        else if (TryMatch(">=")) AddTokenFromCurrent(TokenType.GREATER_THAN_OR_EQUAL, 2);
        else if (c == '<') AddTokenFromCurrent(TokenType.LESS_THAN, 1);
        else if (c == '>') AddTokenFromCurrent(TokenType.GREATER_THAN, 1);
        else if (c == ':') AddTokenFromCurrent(TokenType.COLON, 1);
        else if (c == '|') AddTokenFromCurrent(TokenType.PIPE, 1);
        else if (c != '"' && c != '\'') ReadString(c);
        else if (IsDigit(c))
        {
            // Could be a Number or Identifier

            // 1927
            // 32.14
            // 1stGrade

            var startIndex = this.currentIndex;

            SkipDigits();

            var c2 = CharacterAt(++this.currentIndex);
            if (c2 == '_' || IsLowercase(c2) || IsUppercase(c2))
            {
                SkipIdentifierChars();
                AddToken(new Token(TokenType.IDENTIFIER, startIndex, this.currentIndex - 1));
            }
            else if (c2 == '.')
            {
                SkipDigits();
                AddToken(new Token(TokenType.DECIMAL, startIndex, this.currentIndex - 1));
            }
            else
            {
                AddToken(new Token(TokenType.INTEGER, startIndex, this.currentIndex - 1));
            }

        }
        else if (c == '.')
        {
            // Could be a Number, Range, or Dot
            // .93
            // .. (used as 1..9)
            // . (used as thing.prop)

            var startIndex = this.currentIndex;

            var c2 = CharacterAt(this.currentIndex + 1);
            if (c2 == '.')
            {
                AddTokenFromCurrent(TokenType.RANGE, 2);
            }
            else if (IsDigit(c2))
            {
                SkipDigits();
                AddToken(new Token(TokenType.DECIMAL, startIndex, this.currentIndex - 1));
            }
            else
            {
                AddToken(new Token(TokenType.DOT, startIndex, startIndex));
            }
        }
        else if (c == '-')
        {

            var startIndex = this.currentIndex;

            var c2 = CharacterAt(++this.currentIndex);
            if (IsDigit(c2))
            {
                SkipDigits();
                var c3 = CharacterAt(this.currentIndex);
                if (c3 == '.')
                {
                    SkipDigits();
                    AddToken(new Token(TokenType.DECIMAL, startIndex, this.currentIndex - 1));
                }
                else
                {
                    AddToken(new Token(TokenType.INTEGER, startIndex, this.currentIndex - 1));
                }
            }
            else if (c2 == '.')
            {
                SkipDigits();
                AddToken(new Token(TokenType.DECIMAL, startIndex, this.currentIndex - 1));
            }
            else
            {
                AddToken(new Token(TokenType.RAW_TEXT, startIndex, startIndex));
            }
        }



    }

    private bool TryReadLavaVariableEndOrEOF(int startIndex)
    {

        int endIndex = startIndex;

        int c = CharacterAt(startIndex);
        if (c == '-')
        {

            var c2 = CharacterAt(++endIndex);
            if (c2 == '}')
            {

                var c3 = CharacterAt(++endIndex);
                if (c3 == '}')
                {

                    lexerMode = LexerMode.RAW;
                    AddToken(new Token(TokenType.LAVA_TRIM_WHITESPACE_FLAG, startIndex, startIndex));
                    AddToken(new Token(TokenType.LAVA_VARIABLE_EXIT, startIndex + 1, endIndex));
                    return true;

                }
            }
        }
        else if (c == '}')
        {

            var c2 = CharacterAt(++endIndex);
            if (c2 == '}')
            {

                lexerMode = LexerMode.RAW;
                AddToken(new Token(TokenType.LAVA_VARIABLE_EXIT, startIndex, endIndex));
                return true;

            }
        }
        return false;
    }

    private bool ShiftTokenStack()
    {
        if (this.nextTokens.Count > 0)
        {
            this.currentToken = this.nextTokens.Dequeue();
            this.currentIndex = this.currentToken.EndIndex + 1;
            return true;
        }
        return false;
    }

    public bool ReadNextToken()
    {
        if (this.currentToken != null && this.currentToken.TokenType == TokenType.EOF) return false;

        this.currentToken = null;

        if (ShiftTokenStack()) return true;

        switch (lexerMode)
        {
            case LexerMode.RAW:
                MoveNext_Raw();
                break;
            case LexerMode.LAVA_VARIABLE:
                MoveNext_LavaVariable();
                break;
            default:
                MoveNext_Raw();
                break;
        }


        return this.currentToken != null;
    }

    public void Dispose()
    {
        // Noop
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipWhitespace()
    {
        while (IsWhitespace(CharacterAt(this.currentIndex))) this.currentIndex++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipDigits()
    {
        while (IsDigit(CharacterAt(this.currentIndex))) this.currentIndex++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipIdentifierChars()
    {
        var c = CharacterAt(this.currentIndex);
        while (IsDigit(c) || IsLetter(c) || c == '_')
        {
            c = CharacterAt(++this.currentIndex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(int c)
    {
        return c >= 48 && c <= 57;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEOF(int c)
    {
        return c == -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter(int c)
    {
        return IsUppercase(c) || IsLowercase(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUppercase(int c)
    {
        return c >= 65 && c <= 90;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowercase(int c)
    {
        return c >= 97 && c <= 122;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhitespace(int c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r';
    }
}