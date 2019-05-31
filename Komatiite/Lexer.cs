using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Komatiite
{

    /// <summary>
    /// Lexes input characters into a list of Tokens.
    /// </summary>
    /// <typeparam name="Token"></typeparam>
    public class Lexer : IEnumerable<Token>, IEnumerator<Token>
    {


        #region Fields

        /// <summary>
        /// The ILavaReader used to read in characters.
        /// </summary>
        private ILavaReader reader;

        /// <summary>
        /// A Queue of Tokens that should be returned.
        /// </summary>
        private Queue<Token> tokenQueue = new Queue<Token>();

        /// <summary>
        /// The currently returned token.
        /// </summary>
        private Token currentToken;

        /// <summary>
        /// A Stack of LexerModes.
        /// </summary>
        private Stack<LexerMode> lexerModeStack = new Stack<LexerMode>();

        /// <summary>
        /// The current lexer mode.
        /// </summary>
        public LexerMode CurrentLexerMode => this.currentLexerMode;

        /// <summary>
        /// The depth of the current lexer mode.
        /// </summary>
        public int CurrentLexerModeDepth => this.lexerModeStack.Count;

        /// <summary>
        /// The current LexerMode.
        /// </summary>
        private LexerMode currentLexerMode;

        #endregion


        /// <summary>
        /// Creates a new Lexer that reads tokens from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Lexer(string input) : this(new LavaStringReader(input)) { }


        /// <summary>
        /// Creates a new Lexer that reads tokens from a Stream.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Lexer(Stream input) : this(new LavaStreamReader(input)) { }


        /// <summary>
        /// Creates a new Lexer that reads tokens from an ILavaReader.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Lexer(ILavaReader input) => this.reader = input;



        #region Helpers

        /// <summary>
        /// Adds a new Token to the Queue of Tokens to return.
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        private Token AddToken(Token newToken)
        {
            if (this.currentToken == null) this.currentToken = newToken;
            else this.tokenQueue.Enqueue(newToken);
            return newToken;
        }

        /// <summary>
        /// Creates a new Token and adds it to the Queue of Tokens to return.
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        private Token AddNewToken(TokenType tokenType, CharacterPosition startPosition, int length)
        {
            return AddToken(new Token(tokenType, startPosition, length));
        }

        /// <summary>
        /// Creates a new Token using the current chatacter position, and adds it to the Queue of Tokens to return.
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        private Token AddNewTokenFromCurrent(TokenType tokenType, int length)
        {
            var token = AddToken(new Token(tokenType, reader.CurrentPosition.Clone(), length));
            while (length-- > 0) reader.NextCharacter();
            return token;
        }

        /// <summary>
        /// If the current position has changed, creates a new Token and adds it to the Queue of Tokens to return.
        /// </summary>
        /// <param name="newToken"></param>
        /// <returns></returns>
        private void AddNewTokenIfNeeded(TokenType tokenType, CharacterPosition startPosition)
        {
            if (startPosition < reader.CurrentPosition) AddToken(new Token(tokenType, startPosition, reader.CurrentPosition - startPosition));
        }

        /// <summary>
        /// Tries to dequeue a Token.
        /// </summary>
        /// <returns></returns>
        private bool TryDequeueToken()
        {
            if (this.tokenQueue.Count > 0)
            {
                this.currentToken = this.tokenQueue.Dequeue();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pushes a LexerMode onto the LexerMode Stack.
        /// </summary>
        /// <param name="lexerMode"></param>
        private void PushLexerMode(LexerMode lexerMode)
        {
            this.lexerModeStack.Push(this.currentLexerMode);
            this.currentLexerMode = lexerMode;
        }

        /// <summary>
        /// Pops a LexerMode off of the top of the LexerMode Stack.
        /// </summary>
        private void PopLexerMode()
        {
            if (this.lexerModeStack.Count > 0) this.currentLexerMode = this.lexerModeStack.Pop();
            else this.currentLexerMode = LexerMode.RAW;
        }

        /// <summary>
        /// Replaces the LexerMode at the top of the Lexer Mode Stack.
        /// </summary>
        /// <param name="lexerMode"></param>
        private void ReplaceLexerMode(LexerMode lexerMode)
        {
            PopLexerMode();
            PushLexerMode(lexerMode);
        }

        #endregion



        #region Token Readers

        /// <summary>
        /// Reads the next Token from the input.
        /// </summary>
        /// <returns></returns>
        private bool ReadNextToken()
        {

            // See if we already have a token we can dequeue and return.
            if (TryDequeueToken()) return true;

            // Make sure we're not already at EOF
            if (this.currentToken?.TokenType == TokenType.EOF) return false;

            // Clear the current token
            this.currentToken = null;

            // Read the next token using the appropriate method
            switch (this.currentLexerMode)
            {
                case LexerMode.RAW: ReadNextToken_Raw(); break;
                case LexerMode.LAVA_VARIABLE: ReadNextToken_LavaVariable(); break;
                case LexerMode.LAVA_TAG: ReadNextToken_LavaTag(); break;
                case LexerMode.LAVA_SHORTCODE: ReadNextToken_LavaShortcode(); break;
                case LexerMode.LAVA_INTERPOLATED_STRING: ReadNextToken_InterpolatedString(); break;
                case LexerMode.LAVA_SHORTHAND_COMMENT: ReadNextToken_ShorthandComment(); break;
                case LexerMode.LAVA_SHORTHAND_LITERAL: ReadNextToken_ShorthandLiteral(); break;
                default: ReadNextToken_Raw(); break;
            }

            // If we got another token
            return this.currentToken != null;

        }

        /// <summary>
        /// Reads the next Token from the input in RAW mode.
        /// </summary>
        private void ReadNextToken_Raw()
        {

            var rawStart = reader.CurrentPosition.Clone();

            while (true)
            {

                // Check the current character
                switch (reader.CurrentCharacter)
                {
                    case -1:

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, rawStart);

                        // Add a token for the EOF
                        AddNewTokenFromCurrent(TokenType.EOF, 0);

                        return;

                    case '{':

                        // Check the next character
                        switch (reader.PeekCharacter(1))
                        {
                            case '{':

                                // Check the next next character
                                if (reader.PeekCharacter(2) == '{')
                                {
                                    HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTHAND_LITERAL_ENTER, 3, LexerMode.LAVA_SHORTHAND_LITERAL);
                                }
                                else
                                {
                                    HandleRawLavaStart(rawStart, TokenType.LAVA_VARIABLE_ENTER, 2, LexerMode.LAVA_VARIABLE);
                                }
                                return;

                            case '%':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_TAG_ENTER, 2, LexerMode.LAVA_TAG);
                                return;

                            case '[':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTCODE_ENTER, 2, LexerMode.LAVA_SHORTCODE);
                                return;

                            case '#':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTHAND_COMMENT_ENTER, 2, LexerMode.LAVA_SHORTHAND_COMMENT);
                                return;

                        }
                        break;

                }

                // If we didn't get a match, move on to the next character
                reader.NextCharacter();

            }


        }

        /// <summary>
        /// Handles lava start tokens when ReadNextToken_Raw finds one.
        /// </summary>
        /// <param name="rawStart">The start position of the raw text.</param>
        /// <param name="tokenType">The type of the start token.</param>
        /// <param name="tokenSize">The size of the start token.</param>
        /// <param name="newLexerMode">The lexer mode to switch to for reading the lava.</param>
        private void HandleRawLavaStart(CharacterPosition rawStart, TokenType tokenType, int tokenSize, LexerMode newLexerMode)
        {

            // Add a token for the raw text if needed
            AddNewTokenIfNeeded(TokenType.RAW_TEXT, rawStart);

            // Add a token for the lava start
            AddNewTokenFromCurrent(tokenType, tokenSize);

            // Check for a whitespace modifier
            if (reader.CurrentCharacter == '-')
            {
                // Add the token
                AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

            }

            // Update the lexer mode
            PushLexerMode(newLexerMode);

        }

        /// <summary>
        /// Reads the next Token from the input in LAVA_VARIABLE mode.
        /// </summary>
        private void ReadNextToken_LavaVariable()
        {
            ReadNextGenericLavaToken();
        }

        /// <summary>
        /// Reads the next Token from the input in LAVA_TAG mode.
        /// </summary>
        private void ReadNextToken_LavaTag()
        {
            ReadNextGenericLavaToken();
        }

        /// <summary>
        /// Reads the next Token from the input in LAVA_SHORTCODE mode.
        /// </summary>
        private void ReadNextToken_LavaShortcode()
        {
            ReadNextGenericLavaToken();
        }

        /// <summary>
        /// Reads the next lava Token from the input.
        /// </summary>
        private void ReadNextGenericLavaToken()
        {

            // The fallback token will be used if we can't find any character matches
            Token fallbackToken = null;

            while (true)
            {
                var c = reader.CurrentCharacter;

                // Check the current character
                switch (c)
                {
                    case -1: AddNewTokenFromCurrent(TokenType.EOF, 0); return;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n': reader.NextCharacter(); break;
                    case '[': AddNewTokenFromCurrent(TokenType.LEFT_SQUARE_BRACKET, 1); return;
                    case '(': AddNewTokenFromCurrent(TokenType.LEFT_PARENTHESES, 1); return;
                    case ')': AddNewTokenFromCurrent(TokenType.RIGHT_PARENTHESES, 1); return;
                    case ':': AddNewTokenFromCurrent(TokenType.COLON, 1); return;
                    case '|': AddNewTokenFromCurrent(TokenType.PIPE, 1); return;
                    case '"':

                        // Add the string start
                        AddNewTokenFromCurrent(TokenType.STRING_START, 1);

                        // Read the string contents
                        ReadStringContent('"');

                        return;

                    case '\'':

                        // Add the string start
                        AddNewTokenFromCurrent(TokenType.STRING_START, 1);

                        // Check if we're in a shortcode
                        if (currentLexerMode == LexerMode.LAVA_SHORTCODE)
                        {
                            // Read it as an interpolated string
                            PushLexerMode(LexerMode.LAVA_INTERPOLATED_STRING);
                        }
                        else
                        {
                            // Read it as a normal string
                            ReadStringContent('\'');
                        }

                        return;

                    case ']':

                        // Check for the end of a shortcode
                        if (currentLexerMode == LexerMode.LAVA_SHORTCODE && reader.PeekCharacter(1) == '}')
                        {

                            // Add the exit token
                            AddNewTokenFromCurrent(TokenType.LAVA_SHORTCODE_EXIT, 2);

                            // We're done reading the shortcode, so go back to whatever mode we were in before.
                            PopLexerMode();

                            return;

                        }
                        else
                        {

                            // Add a right bracket token
                            AddNewTokenFromCurrent(TokenType.RIGHT_SQUARE_BRACKET, 1);

                            return;

                        }

                    case '%':

                        // Check for the end of a tag
                        if (currentLexerMode == LexerMode.LAVA_TAG && reader.PeekCharacter(1) == '}')
                        {

                            // Add the exit token
                            AddNewTokenFromCurrent(TokenType.LAVA_TAG_EXIT, 2);

                            // We're done reading the tag, so go back to whatever mode we were in before.
                            PopLexerMode();

                            return;

                        }

                        goto default;

                    case '}':

                        // Check for the end of a variable
                        if (currentLexerMode == LexerMode.LAVA_VARIABLE && reader.PeekCharacter(1) == '}')
                        {

                            // Add the exit token
                            AddNewTokenFromCurrent(TokenType.LAVA_VARIABLE_EXIT, 2);

                            // We're done reading the variable, so go back to whatever mode we were in before.
                            PopLexerMode();

                            return;

                        }

                        goto default;

                    case '<':

                        if (reader.PeekCharacter(1) == '=')
                        {
                            AddNewTokenFromCurrent(TokenType.LESS_THAN_OR_EQUAL, 2);
                        }
                        else
                        {
                            AddNewTokenFromCurrent(TokenType.LESS_THAN, 1);
                        }

                        return;

                    case '>':

                        if (reader.PeekCharacter(1) == '=')
                        {
                            AddNewTokenFromCurrent(TokenType.GREATER_THAN_OR_EQUAL, 2);
                        }
                        else
                        {
                            AddNewTokenFromCurrent(TokenType.GREATER_THAN, 1);
                        }

                        return;

                    case '=':

                        if (reader.PeekCharacter(1) == '=')
                        {
                            AddNewTokenFromCurrent(TokenType.EQUALS, 2);
                        }
                        else
                        {
                            AddNewTokenFromCurrent(TokenType.ASSIGNMENT, 1);
                        }

                        return;

                    case '!':

                        if (reader.PeekCharacter(1) == '=')
                        {
                            AddNewTokenFromCurrent(TokenType.NOT_EQUAL, 2);
                            return;
                        }

                        goto default;

                    case '.':

                        var dotNextChar = reader.PeekCharacter(1);

                        if (dotNextChar == '.')
                        {
                            AddNewTokenFromCurrent(TokenType.RANGE, 2);
                        }
                        else if (CharacterUtil.IsDigit(dotNextChar))
                        {
                            ReadNumber();
                        }
                        else
                        {
                            AddNewTokenFromCurrent(TokenType.DOT, 1);
                        }

                        return;

                    case '-':

                        var minusNextChar = reader.PeekCharacter(1);

                        switch (currentLexerMode)
                        {
                            case LexerMode.LAVA_SHORTCODE:

                                // Check for the end of a shortcode
                                if (minusNextChar == ']' && reader.PeekCharacter(2) == '}')
                                {

                                    // Add the whitespace modifier
                                    AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

                                    // Add the exit token
                                    AddNewTokenFromCurrent(TokenType.LAVA_SHORTCODE_EXIT, 2);

                                    // We're done reading the shortcode, so go back to whatever mode we were in before.
                                    PopLexerMode();

                                    return;

                                }

                                goto default;

                            case LexerMode.LAVA_TAG:

                                // Check for the end of a tag
                                if (minusNextChar == '%' && reader.PeekCharacter(2) == '}')
                                {

                                    // Add the whitespace modifier
                                    AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

                                    // Add the exit token
                                    AddNewTokenFromCurrent(TokenType.LAVA_TAG_EXIT, 2);

                                    // We're done reading the tag, so go back to whatever mode we were in before.
                                    PopLexerMode();

                                    return;

                                }

                                goto default;

                            case LexerMode.LAVA_VARIABLE:

                                // Check for the end of a variable
                                if (minusNextChar == '}' && reader.PeekCharacter(2) == '}')
                                {

                                    // Add the whitespace modifier
                                    AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

                                    // Add the exit token
                                    AddNewTokenFromCurrent(TokenType.LAVA_VARIABLE_EXIT, 2);

                                    // We're done reading the tag, so go back to whatever mode we were in before.
                                    PopLexerMode();

                                    return;

                                }

                                goto default;

                            default:

                                if (CharacterUtil.IsDigit(minusNextChar) || minusNextChar == '.')
                                {
                                    ReadNumber();
                                    return;
                                }

                                break;

                        }

                        goto default;

                    default:

                        if (CharacterUtil.IsDigit(c))
                        {
                            ReadNumber();
                            return;
                        }
                        else if (CharacterUtil.IsIdentifierStart(c))
                        {
                            // Start an identifier token
                            var startPosition = reader.CurrentPosition.Clone();

                            // Consume characters until we get to a non-identifier character
                            while (CharacterUtil.IsIdentifierPart(reader.NextCharacter())) { }

                            // Add the identifier token
                            AddNewToken(TokenType.IDENTIFIER, startPosition, reader.CurrentPosition - startPosition);

                            return;

                        }

                        // If nothing matched, fall back to a raw token
                        else if (fallbackToken == null)
                        {
                            fallbackToken = AddNewTokenFromCurrent(TokenType.RAW_TEXT, 0);
                        }
                        else
                        {
                            fallbackToken.Length++;
                        }

                        reader.NextCharacter();

                        break;
                }

            }

        }

        /// <summary>
        /// Reads the contents of a string token.
        /// </summary>
        /// <param name="endChar"></param>
        private void ReadStringContent(int endChar)
        {

            // Save the start position
            var startPosition = reader.CurrentPosition.Clone();

            // Check the current character
            while (true)
            {
                switch (reader.CurrentCharacter)
                {
                    case -1:

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                        // Add a token for the EOF
                        AddNewTokenFromCurrent(TokenType.EOF, 0);

                        return;

                    case '\\':

                        // Skip the escaped character
                        reader.NextCharacter();
                        reader.NextCharacter();

                        break;

                    default:

                        // Check if it's the end of the string
                        if (reader.CurrentCharacter == endChar)
                        {

                            // Add a token for the raw text if needed
                            AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                            // Add a token for the string end
                            AddNewTokenFromCurrent(TokenType.STRING_END, 1);

                            return;

                        }

                        // If we didn't get a match, move on to the next character
                        reader.NextCharacter();

                        break;

                }

            }

        }


        private void ReadNumber()
        {

            var c = reader.CurrentCharacter;
            var token = AddNewTokenFromCurrent(TokenType.INTEGER, 0);

            // Possible negative sign
            if (c == '-')
            {
                c = reader.NextCharacter();
            }

            // Digits
            while (CharacterUtil.IsDigit(c))
            {
                c = reader.NextCharacter();
            }

            // Possible decimal point
            if (c == '.')
            {
                token.TokenType = TokenType.DECIMAL;
                c = reader.NextCharacter();
            }

            // Digits
            while (CharacterUtil.IsDigit(c))
            {
                c = reader.NextCharacter();
            }

            token.Length = reader.CurrentPosition.Index - token.StartPosition.Index;
        }

        /// <summary>
        /// Reads the next Token from the input in INTERPOLATED_STRING mode.
        /// </summary>
        private void ReadNextToken_InterpolatedString()
        {

            // Save the start position
            var rawStart = reader.CurrentPosition.Clone();

            // Check the current character
            while (true)
            {
                switch (reader.CurrentCharacter)
                {
                    case -1:

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, rawStart);

                        // Add a token for the EOF
                        AddNewTokenFromCurrent(TokenType.EOF, 0);

                        return;

                    case '\\':

                        // Skip the escaped character
                        reader.NextCharacter();
                        reader.NextCharacter();

                        break;

                    case '{':

                        // Check the next character
                        switch (reader.PeekCharacter(1))
                        {
                            case '{':

                                // Check the next next character
                                if (reader.PeekCharacter(2) == '{')
                                {
                                    HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTHAND_LITERAL_ENTER, 3, LexerMode.LAVA_SHORTHAND_LITERAL);
                                }
                                else
                                {
                                    HandleRawLavaStart(rawStart, TokenType.LAVA_VARIABLE_ENTER, 2, LexerMode.LAVA_VARIABLE);
                                }
                                return;

                            case '%':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_TAG_ENTER, 2, LexerMode.LAVA_TAG);
                                return;

                            case '[':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTCODE_ENTER, 2, LexerMode.LAVA_SHORTCODE);
                                return;

                            case '#':
                                HandleRawLavaStart(rawStart, TokenType.LAVA_SHORTHAND_COMMENT_ENTER, 2, LexerMode.LAVA_SHORTHAND_COMMENT);
                                return;

                        }

                        goto default;

                    case '\'':

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, rawStart);

                        // Add a token for the string end
                        AddNewTokenFromCurrent(TokenType.STRING_END, 1);
                        
                        // We're done reading the string, so go back to whatever mode we were in before.
                        PopLexerMode();

                        return;

                    default:

                        // If we didn't get a match, move on to the next character
                        reader.NextCharacter();

                        break;

                }

            }

        }

        /// <summary>
        /// Reads the next Token from the input in LAVA_SHORTHAND_COMMENT mode.
        /// </summary>
        private void ReadNextToken_ShorthandComment()
        {

            // Save the start position
            var startPosition = reader.CurrentPosition.Clone();

            // Check the current character
            while (true)
            {
                switch (reader.CurrentCharacter)
                {
                    case -1:

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                        // Add a token for the EOF
                        AddNewTokenFromCurrent(TokenType.EOF, 0);

                        return;

                    case '#':

                        if (reader.PeekCharacter(1) == '}')
                        {

                            // Add a token for the raw text if needed
                            AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                            // Add a token for the shorthand comment end
                            AddNewTokenFromCurrent(TokenType.LAVA_SHORTHAND_COMMENT_EXIT, 2);
                        
                            // We're done reading the shorthand comment, so go back to whatever mode we were in before.
                            PopLexerMode();

                            return;

                        }

                        goto default;

                    case '-':

                        if (reader.PeekCharacter(1) == '#')
                        {
                            if (reader.PeekCharacter(2) == '}')
                            {

                                // Add a token for the raw text if needed
                                AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                                // Add a token for the whitespace modifier
                                AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

                                // Add a token for the shorthand comment end
                                AddNewTokenFromCurrent(TokenType.LAVA_SHORTHAND_COMMENT_EXIT, 2);
                        
                                // We're done reading the shorthand comment, so go back to whatever mode we were in before.
                                PopLexerMode();

                                return;

                            }
                        }

                        goto default;

                    default:

                        // We didn't get a match, move on to the next character
                        reader.NextCharacter();

                        break;

                }

            }
        }

        /// <summary>
        /// Reads the next Token from the input in LAVA_SHORTHAND_LITERAL mode.
        /// </summary>
        private void ReadNextToken_ShorthandLiteral()
        {

            // Save the start position
            var startPosition = reader.CurrentPosition.Clone();

            // Check the current character
            while (true)
            {
                switch (reader.CurrentCharacter)
                {
                    case -1:

                        // Add a token for the raw text if needed
                        AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                        // Add a token for the EOF
                        AddNewTokenFromCurrent(TokenType.EOF, 0);

                        return;

                    case '}':

                        if (reader.PeekCharacter(1) == '}')
                        {
                            if (reader.PeekCharacter(2) == '}')
                            {

                                // Add a token for the raw text if needed
                                AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                                // Add a token for the shorthand comment end
                                AddNewTokenFromCurrent(TokenType.LAVA_SHORTHAND_LITERAL_EXIT, 3);
                        
                                // We're done reading the shorthand literal, so go back to whatever mode we were in before.
                                PopLexerMode();

                                return;

                            }
                        }

                        goto default;

                    case '-':

                        if (reader.PeekCharacter(1) == '}')
                        {
                            if (reader.PeekCharacter(2) == '}')
                            {
                                if (reader.PeekCharacter(3) == '}')
                                {

                                    // Add a token for the raw text if needed
                                    AddNewTokenIfNeeded(TokenType.RAW_TEXT, startPosition);

                                    // Add a token for the whitespace modifier
                                    AddNewTokenFromCurrent(TokenType.LAVA_TRIM_WHITESPACE_FLAG, 1);

                                    // Add a token for the shorthand comment end
                                    AddNewTokenFromCurrent(TokenType.LAVA_SHORTHAND_LITERAL_EXIT, 3);
                        
                                    // We're done reading the shorthand literal, so go back to whatever mode we were in before.
                                    PopLexerMode();

                                    return;

                                }
                            }
                        }

                        goto default;

                    default:

                        // We didn't get a match, move on to the next character
                        reader.NextCharacter();

                        break;

                }

            }
        }

        #endregion



        #region IEnumerable<Token>

        public IEnumerator<Token> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        #endregion



        #region IEnumerator<Token>

        public Token Current => currentToken;

        object IEnumerator.Current => currentToken;

        public void Dispose() { }

        public bool MoveNext() => ReadNextToken();

        public void Reset() { }


        #endregion
    }
}