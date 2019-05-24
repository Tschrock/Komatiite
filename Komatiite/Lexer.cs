
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

namespace Komatiite
{

    public class Lexer : IEnumerable<Token>, IEnumerator<Token>
    {

        #region Fields

        private ILavaReader reader;

        private LexerMode lexerMode;

        private int lexerModeModifier;

        private Stack<KeyValuePair<LexerMode, int>> lexerModeStack = new Stack<KeyValuePair<LexerMode, int>>();

        private Token currentToken;

        private Token previousToken;

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

            while (tokenLength-- > 1)
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

            if (TryReadEOF()) return true;

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
                        lexerModeStack.Push(new KeyValuePair<LexerMode, int>(LexerMode.LAVA_SHORTHAND_LITERAL, -1));

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
                        lexerModeStack.Push(new KeyValuePair<LexerMode, int>(LexerMode.LAVA_VARIABLE, -1));

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
                    lexerModeStack.Push(new KeyValuePair<LexerMode, int>(LexerMode.LAVA_TAG, -1));

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
                    lexerModeStack.Push(new KeyValuePair<LexerMode, int>(LexerMode.LAVA_SHORTCODE, -1));

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
                    lexerModeStack.Push(new KeyValuePair<LexerMode, int>(LexerMode.LAVA_SHORTHAND_COMMENT, -1));

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
                AddToken(TokenType.LAVA_TRIM_WHITESPACE_FLAG, startPosition, startPosition);

                // Consume the character
                reader.NextCharacter();

                return true;

            }

            return false;

        }

        private void ReadStringContent(int quote)
        {

            int c = reader.CurrentCharacter;
            var stringStart = reader.CurrentPosition.Clone();
            var stringEnd = reader.CurrentPosition.Clone();
            while (true)
            {
                switch (c)
                {
                    // EOF
                    case -1:

                        // If we collected any text, add it
                        if (stringStart.Index < stringEnd.Index) AddToken(TokenType.RAW_TEXT, stringStart, stringEnd);

                        // Add an EOF
                        AddToken(TokenType.EOF, CharacterPosition.Empty, CharacterPosition.Empty);

                        return;

                    // Escape
                    case '\\':

                        // Skip the backslash character
                        stringEnd.BumpForChar(reader.NextCharacter());

                        // Skip the escaped character
                        stringEnd.BumpForChar(c = reader.NextCharacter());

                        break;

                    // Other
                    default:

                        // Check for the end quote
                        if (c == quote)
                        {
                            // If we collected any text, add it
                            if (stringStart.Index < stringEnd.Index) AddToken(TokenType.RAW_TEXT, stringStart, stringEnd);

                            // Add a String End
                            AddTokenAndNext(TokenType.STRING_END, 1);

                            return;
                        }

                        // Move to the next character
                        stringEnd.BumpForChar(c = reader.NextCharacter());

                        break;
                }
            }

        }

        private void pushMode(LexerMode mode, int modifier)
        {
            lexerModeStack.Push(new KeyValuePair<LexerMode, int>(mode, modifier));
        }

        private void ReadLava()
        {

            int c = reader.CurrentCharacter;
            int c2 = -1;
            Token fallbackToken = null;
            while (true)
            {

                switch (c)
                {
                    case ']':
                        if (lexerMode == LexerMode.LAVA_SHORTCODE)
                        {
                            c2 = reader.PeekCharacter(1);
                            if (c2 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_SHORTCODE_EXIT, 2);
                                return;
                            }
                        }
                        AddTokenAndNext(TokenType.RIGHT_SQUARE_BRACKET);
                        return;
                    case '%':
                        if (lexerMode == LexerMode.LAVA_TAG)
                        {
                            c2 = reader.PeekCharacter(1);
                            if (c2 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_TAG_EXIT, 2);
                                return;
                            }
                        }
                        goto default;
                    case '}':
                        if (lexerMode == LexerMode.LAVA_VARIABLE)
                        {
                            c2 = reader.PeekCharacter(1);
                            if (c2 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_VARIABLE_EXIT, 2);
                                return;
                            }
                        }
                        goto default;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        c = reader.NextCharacter();
                        break;
                    case -1: AddToken(TokenType.EOF, CharacterPosition.Empty, CharacterPosition.Empty); return;
                    case '[': AddTokenAndNext(TokenType.LEFT_SQUARE_BRACKET); return;
                    case '(': AddTokenAndNext(TokenType.LEFT_PARENTHESES); return;
                    case ')': AddTokenAndNext(TokenType.RIGHT_PARENTHESES); return;
                    case ':': AddTokenAndNext(TokenType.COLON); return;
                    case '|': AddTokenAndNext(TokenType.PIPE); return;
                    case '"':
                    case '\'':

                        AddTokenAndNext(TokenType.STRING_START);
                    
                        if (lexerMode == LexerMode.LAVA_SHORTCODE) pushMode(LexerMode.LAVA_INTERPOLATED_STRING, c);
                        else ReadStringContent(c);

                        return;
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
                            AddTokenAndNext(TokenType.NOT_EQUAL, 2);
                            return;
                        }

                        goto default;
                    case '.':
                        c2 = reader.PeekCharacter(1);

                        if (c2 == '.') AddTokenAndNext(TokenType.RANGE, 2);
                        else if (IsDigit(c2)) ReadNumber();
                        else AddTokenAndNext(TokenType.DOT);

                        return;
                    case '-':
                        c2 = reader.PeekCharacter(1);

                        if (lexerMode == LexerMode.LAVA_VARIABLE && c2 == '}')
                        {
                            var c3 = reader.PeekCharacter(2);
                            if (c3 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);
                                AddTokenAndNext(TokenType.LAVA_VARIABLE_EXIT, 2);
                                return;
                            }
                        }
                        else if (lexerMode == LexerMode.LAVA_TAG && c2 == '%')
                        {
                            var c3 = reader.PeekCharacter(2);
                            if (c3 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);
                                AddTokenAndNext(TokenType.LAVA_TAG_EXIT, 2);
                                return;
                            }
                        }
                        else if (lexerMode == LexerMode.LAVA_SHORTCODE && c2 == ']')
                        {
                            var c3 = reader.PeekCharacter(2);
                            if (c3 == '}')
                            {
                                lexerModeStack.Pop();
                                AddTokenAndNext(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);
                                AddTokenAndNext(TokenType.LAVA_SHORTCODE_EXIT, 2);
                                return;
                            }
                        }
                        else if (IsDigit(c2) || c2 == '.')
                        {
                            ReadNumber();
                        }
                        else
                        {
                            AddTokenAndNext(TokenType.DOT);
                        }

                        return;
                    default:
                        if (IsDigit(c))
                        {
                            ReadNumber();
                            return;
                        }
                        else if (IsIdentifierChar(c))
                        {
                            var token = AddToken(TokenType.IDENTIFIER);
                            c = reader.NextCharacter();
                            while (IsIdentifierChar(c))
                            {
                                token.EndPosition.BumpColumn();
                                c = reader.NextCharacter();
                            }
                            return;
                        }
                        else if (fallbackToken == null)
                        {
                            fallbackToken = AddToken(TokenType.RAW_TEXT);
                        }
                        else {
                            fallbackToken.EndPosition.BumpForChar(c);
                            c = reader.NextCharacter();
                        }

                        break;
                }

            }

        }

        private void ReadNumber()
        {
            var c = reader.CurrentCharacter;
            var token = AddToken(TokenType.INTEGER);

            // Possible negative sign
            if (c == '-')
            {
                token.EndPosition.Index++;
                c = reader.NextCharacter();
            }

            // Digits
            while (IsDigit(c))
            {
                token.EndPosition.Index++;
                c = reader.NextCharacter();
            }

            // Possible decimal point
            if (c == '.')
            {
                token.TokenType = TokenType.DECIMAL;
                token.EndPosition.Index++;
                c = reader.NextCharacter();
            }

            // Digits
            while (IsDigit(c))
            {
                token.EndPosition.Index++;
                c = reader.NextCharacter();
            }

        }
        
        private void ReadInterpolated()
        {

            // Check for the end of the string
            if (reader.CurrentCharacter == lexerModeModifier)
            {
                AddTokenAndNext(TokenType.STRING_END, 1);
                this.lexerModeStack.Pop();
                return;
            }

            // Check for a lava token or EOF
            if (TryReadLavaStartOrEOF()) return;

            // Start a RAW_TEXT token
            var rawToken = AddToken(TokenType.RAW_TEXT);

            // Loop forward until we find a lava or EOF token
            while (true)
            {
                // Check for an escaped character
                if (reader.CurrentCharacter == '\\')
                {
                    reader.NextCharacter();
                    reader.NextCharacter();
                }


                // Check for the end of the string
                if (reader.CurrentCharacter == lexerModeModifier)
                {
                    AddTokenAndNext(TokenType.STRING_END, 1);
                    this.lexerModeStack.Pop();
                    return;
                }

                // Check for a lava token or EOF
                if (TryReadLavaStartOrEOF()) return;

                // Add the character to the raw text token
                rawToken.EndPosition.BumpForChar(reader.CurrentCharacter);

                // Consume the character
                reader.NextCharacter();

            }
        }

        private bool ShiftTokenStack()
        {
            if (this.nextTokens.Count > 0)
            {
                this.currentToken = this.nextTokens.Dequeue();
                return true;
            }
            return false;
        }

        public bool ReadNextToken()
        {
            if (this.currentToken != null && this.currentToken.TokenType == TokenType.EOF) return false;

            this.previousToken = this.currentToken;
            this.currentToken = null;

            if (ShiftTokenStack()) return true;

            if (lexerModeStack.Count > 0)
            {
                var kv = lexerModeStack.Peek();
                lexerMode = kv.Key;
                lexerModeModifier = kv.Value;
            }
            else
            {
                lexerMode = LexerMode.RAW;
                lexerModeModifier = -1;
            }

            switch (lexerMode)
            {
                case LexerMode.RAW:
                    ReadRaw();
                    break;
                case LexerMode.LAVA_VARIABLE:
                case LexerMode.LAVA_TAG:
                case LexerMode.LAVA_SHORTCODE:
                    ReadLava();
                    break;
                case LexerMode.LAVA_INTERPOLATED_STRING:
                    ReadInterpolated();
                    break;
                default:
                    ReadRaw();
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
        private static bool IsIdentifierChar(int c)
        {
            return IsDigit(c) || IsLetter(c) || c == '_';
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

}