
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


    private Token AddToken(TokenType type, CharacterPosition startPosition, CharacterPosition endPosition)
    {
        var token = new Token(type, startPosition, endPosition);
        AddToken(token);
        return token;
    }

    private void AddToken(Token newToken)
    {
        if (this.currentToken == null) this.currentToken = newToken;
        else this.nextTokens.Enqueue(newToken);
    }

    private Token AddToken(TokenType type)
    {
        return AddToken(type, reader.CurrentPosition.Clone(), reader.CurrentPosition.Clone());
    }

    private Token AddTokenAndNext(TokenType type)
    {
        var token = AddToken(type, reader.CurrentPosition.Clone(), reader.CurrentPosition.Clone());
        reader.NextCharacter();
        return token;
    }

    private Token AddTokenAndNext(TokenType type, int tokenLength)
    {

        var token = AddToken(type, reader.CurrentPosition.Clone(), reader.CurrentPosition.Clone());

        while (tokenLength --> 1)
        {
            token.EndPosition.BumpForChar(reader.CurrentCharacter);
            reader.NextCharacter();
        }

        reader.NextCharacter();

        return token;

    }

    private void ReadRaw()
    {

        // Check for a lava token or EOF
        if (TryReadLavaStartOrEOF()) return;

        // Start a RAW_TEXT token
        var rawToken = AddToken(TokenType.RAW_TEXT);

        // Consume the first character
        reader.NextCharacter();

        // Loop forward until we find a lava or EOF token
        while (!TryReadLavaStartOrEOF())
        {
            // Add the character to the raw text token
            rawToken.EndPosition.BumpForChar(reader.CurrentCharacter);

            // Consume the character
            reader.NextCharacter();

        }

    }

    private bool TryReadEOF()
    {
        // Get the current character
        int c = reader.CurrentCharacter;

        // Check for EOF
        if (IsEOF(c))
        {

            // Add the token
            AddToken(TokenType.EOF, CharacterPosition.Empty, CharacterPosition.Empty);

            // Consume the character
            reader.NextCharacter();

            // return
            return true;

        }

        return false;

    }
    private bool TryReadLavaStartOrEOF()
    {
        // Get the current character
        int c = reader.CurrentCharacter;

        // Check if it might be lava
        if (c == '{')
        {
            // Peek ahead
            int c2 = reader.PeekCharacter(1);

            if (c2 == '{')
            {

                // Record the start position
                var startPosition = reader.CurrentPosition.Clone();

                // Consume the first character
                reader.NextCharacter();

                // Check if it's a shorthand literal
                int c3 = reader.PeekCharacter(1);
                if (c3 == '{')
                {

                    // Consume the second character
                    reader.NextCharacter();

                    // Set the lexer mode
                    lexerMode = LexerMode.LAVA_SHORTHAND_LITERAL;

                    // Add the token
                    AddToken(TokenType.LAVA_SHORTHAND_LITERAL_ENTER, startPosition, reader.CurrentPosition.Clone());

                    // Consume the third character
                    reader.NextCharacter();

                    // Check if there's a whitespace modifier
                    TryReadWhiteSpaceModifier();

                    // return
                    return true;

                }
                else
                {

                    // Set the lexer mode
                    lexerMode = LexerMode.LAVA_VARIABLE;

                    // Add the token
                    AddToken(TokenType.LAVA_VARIABLE_ENTER, startPosition, reader.CurrentPosition.Clone());

                    // Consume the second character
                    reader.NextCharacter();

                    // Check if there's a whitespace modifier
                    TryReadWhiteSpaceModifier();

                    // return
                    return true;

                }

            }
            else if (c2 == '%')
            {

                // Record the start position
                var startPosition = reader.CurrentPosition.Clone();

                // Consume the first character
                reader.NextCharacter();

                // Set the lexer mode
                lexerMode = LexerMode.LAVA_TAG;

                // Add the token
                AddToken(TokenType.LAVA_TAG_ENTER, startPosition, reader.CurrentPosition.Clone());

                // Consume the second character
                reader.NextCharacter();

                // Check if there's a whitespace modifier
                TryReadWhiteSpaceModifier();

                // return
                return true;

            }
            else if (c2 == '[')
            {

                // Record the start position
                var startPosition = reader.CurrentPosition.Clone();

                // Consume the first character
                reader.NextCharacter();

                // Set the lexer mode
                lexerMode = LexerMode.LAVA_SHORTCODE;

                // Add the token
                AddToken(TokenType.LAVA_SHORTCODE_ENTER, startPosition, reader.CurrentPosition.Clone());

                // Consume the second character
                reader.NextCharacter();

                // Check if there's a whitespace modifier
                TryReadWhiteSpaceModifier();

                // return
                return true;

            }
            else if (c2 == '#')
            {

                // Record the start position
                var startPosition = reader.CurrentPosition.Clone();

                // Consume the first character
                reader.NextCharacter();

                // Set the lexer mode
                lexerMode = LexerMode.LAVA_SHORTHAND_COMMENT;

                // Add the token
                AddToken(TokenType.LAVA_SHORTHAND_COMMENT_ENTER, startPosition, reader.CurrentPosition.Clone());

                // Consume the second character
                reader.NextCharacter();

                // Check if there's a whitespace modifier
                TryReadWhiteSpaceModifier();

                // return
                return true;

            }
        }
        return false;
    }

    private bool TryReadWhiteSpaceModifier()
    {

        // Check the current character
        int c = reader.CurrentCharacter;
        if (c == '-')
        {

            // Record the start position
            var startPosition = reader.CurrentPosition.Clone();

            // Add the token
            AddToken(TokenType.LAVA_SHORTHAND_LITERAL_ENTER, startPosition, startPosition);

            // Consume the character
            reader.NextCharacter();

            return true;

        }

        return false;

    }

    private bool TryMatch(char[] text, out CharacterPosition startPosition, out CharacterPosition endPosition)
    {
        // Loop through each character
        for (var i = 0; i < text.Length; i++)
        {

            // Check for a match
            if (reader.PeekCharacter(i) != text[i])
            {
                startPosition = CharacterPosition.Empty;
                endPosition = CharacterPosition.Empty;
                return false;
            }

        }

        // Record the start position
        startPosition = reader.CurrentPosition.Clone();

        // Consume all but the last matched characters
        for (var i = 1; i < text.Length; i++) reader.NextCharacter();

        // Record the start position
        endPosition = reader.CurrentPosition.Clone();

        // Consume the last matched character
        reader.NextCharacter();

        return true;
    }

    private void ReadStringContent(int quote)
    {

        // Check for EOF
        if (TryReadEOF()) return;

        // Get the current character
        var c = reader.CurrentCharacter;

        // If we already have a quote
        if (c == quote)
        {

            // Add an end token
            AddToken(TokenType.STRING_END);

            // Consume the character
            reader.NextCharacter();

            // return
            return;

        }

        // Start a RAW_TEXT token
        var rawToken = AddToken(TokenType.RAW_TEXT);

        // Loop until we hit the end quote or EOF
        while (true)
        {

            // Get the next character
            c = reader.NextCharacter();

            // Check for EOF
            if (TryReadEOF()) return;

            // Check for the end quote
            if (c == quote)
            {

                // Add an end token
                AddToken(TokenType.STRING_END);

                // Consume the character
                reader.NextCharacter();

                // return
                return;

            }
            else
            {

                // Add the character to the raw text token
                rawToken.EndPosition.BumpForChar(reader.CurrentCharacter);

            }

        }

    }

    private void ReadLavaVariable()
    {

        SkipWhitespace();

        int c = reader.CurrentCharacter;
        int c2;

        switch (c)
        {
            case '[': AddTokenAndNext(TokenType.LEFT_SQUARE_BRACKET); return;
            case ']': AddTokenAndNext(TokenType.RIGHT_SQUARE_BRACKET); return;
            case '(': AddTokenAndNext(TokenType.LEFT_PARENTHESES); return;
            case ')': AddTokenAndNext(TokenType.RIGHT_PARENTHESES); return;
            case ':': AddTokenAndNext(TokenType.COLON); return;
            case '|': AddTokenAndNext(TokenType.PIPE); return;
            case '"': AddTokenAndNext(TokenType.STRING_START); ReadStringContent('"'); return;
            case '\'': AddTokenAndNext(TokenType.STRING_START); ReadStringContent('\''); return;
            case '<':

                c2 = reader.PeekCharacter(1);
                if (c2 == '=') AddTokenAndNext(TokenType.LESS_THAN_OR_EQUAL, 2);
                else AddTokenAndNext(TokenType.LESS_THAN);

                return;
            case '>':

                c2 = reader.PeekCharacter(1);
                if (c2 == '=') AddTokenAndNext(TokenType.GREATER_THAN_OR_EQUAL, 2);
                else AddTokenAndNext(TokenType.GREATER_THAN);

                return;
            case '=':

                c2 = reader.PeekCharacter(1);
                if (c2 == '=') AddTokenAndNext(TokenType.EQUALS, 2);
                else AddTokenAndNext(TokenType.ASSIGNMENT);

                return;
            case '!':

                c2 = reader.PeekCharacter(1);
                if (c2 == '=')
                {
                    AddTokenAndNext(TokenType.EQUALS, 2);
                    return;
                }

                break;
            case '.':

                c2 = reader.PeekCharacter(1);
                if (c2 == '.')
                {
                    AddTokenAndNext(TokenType.RANGE, 2);
                    return;
                }
                else if (IsDigit(c2))
                {
                    // We need to figure out if it's a number (.1234) or a dot and an identifier (.234test) 
                    var startPosition = reader.CurrentPosition.Clone();

                }
                else
                {
                    AddTokenAndNext(TokenType.DOT, 1);
                }

                break;
            case '-':

                break;
            case '}':

                break;
            default:



                break;
        }

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
        while (IsWhitespace(reader.CurrentCharacter)) reader.NextCharacter();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipDigits()
    {
        while (IsDigit(reader.CurrentCharacter)) reader.NextCharacter();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipIdentifierChars()
    {
        var c = reader.CurrentCharacter;
        while (IsDigit(c) || IsLetter(c) || c == '_') c = reader.NextCharacter();
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