using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Komatiite.Tests
{
    public class LexerTests
    {
        [NiceFact]
        public void Lexer_Should_Lex_Lava_Variable_Start()
        {
            var testString = "{{";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Text_With_Lava_Variable_Start()
        {
            var testString = "Test {{";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Lava_Variable_Start_With_Whitespace_Trim()
        {
            var testString = "{{-";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_TRIM_WHITESPACE_FLAG, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Lava_Variable_Start_And_End()
        {
            var testString = "{{}}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Lava_Variable_Start_And_End_With_Text()
        {
            var testString = "{{}} test";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier()
        {
            var testString = "{{ test }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier_With_Dot()
        {
            var testString = "{{ test.thing }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.DOT, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier_With_Integer_Indexer()
        {
            var testString = "{{ test[0] }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier_With_String_Indexer()
        {
            var testString = "{{ test[\"test\"] }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier_With_Identifier_Indexer()
        {
            var testString = "{{ test[test] }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Identifier_With_Identifier_Indexer_With_Dot()
        {
            var testString = "{{ test[test].test }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.DOT, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Integer()
        {
            var testString = "{{ 0 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Negative_Integer()
        {
            var testString = "{{ 0 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Decimal()
        {
            var testString = "{{ 0.123 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Negative_Decimal()
        {
            var testString = "{{ -0.123 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Decimal_With_Starting_Dot()
        {
            var testString = "{{ .123 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Decimal_With_Negative_Sign_And_Starting_Dot()
        {
            var testString = "{{ -.123 }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Double_Quoted_String()
        {
            var testString = "{{ \"test\" }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Single_Quoted_String()
        {
            var testString = "{{ 'test' }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Double_Quoted_String_With_Escaped_Quote()
        {
            var testString = "{{ \"te\\\"st\" }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceFact]
        public void Lexer_Should_Lex_Single_Quoted_String_With_Escaped_Quote()
        {
            var testString = "{{ 'te\\'st' }}";

            var lexer = new Lexer(testString);

            Assert.Collection(
                lexer.ToList(),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

    }
}
