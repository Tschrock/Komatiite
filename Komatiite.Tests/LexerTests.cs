using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Komatiite.Tests
{
    public class LexerTests
    {
        [NiceTheory]
        [InlineData("{{", TokenType.LAVA_VARIABLE_ENTER)]
        [InlineData("{%", TokenType.LAVA_TAG_ENTER)]
        [InlineData("{[", TokenType.LAVA_SHORTCODE_ENTER)]
        [InlineData("{{{", TokenType.LAVA_SHORTHAND_LITERAL_ENTER)]
        [InlineData("{#", TokenType.LAVA_SHORTHAND_COMMENT_ENTER)]
        public void Lexer_Should_Lex_Lava_Start(string testString, TokenType expectedToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("Test {{", TokenType.LAVA_VARIABLE_ENTER)]
        [InlineData("Test {%", TokenType.LAVA_TAG_ENTER)]
        [InlineData("Test {[", TokenType.LAVA_SHORTCODE_ENTER)]
        [InlineData("Test {{{", TokenType.LAVA_SHORTHAND_LITERAL_ENTER)]
        [InlineData("Test {#", TokenType.LAVA_SHORTHAND_COMMENT_ENTER)]
        public void Lexer_Should_Lex_Text_With_Lava_Start(string testString, TokenType expectedToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(expectedToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{-", TokenType.LAVA_VARIABLE_ENTER)]
        [InlineData("{%-", TokenType.LAVA_TAG_ENTER)]
        [InlineData("{[-", TokenType.LAVA_SHORTCODE_ENTER)]
        [InlineData("{{{-", TokenType.LAVA_SHORTHAND_LITERAL_ENTER)]
        [InlineData("{#-", TokenType.LAVA_SHORTHAND_COMMENT_ENTER)]
        public void Lexer_Should_Lex_Lava_Start_With_Whitespace_Trim(string testString, TokenType expectedToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedToken, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_TRIM_WHITESPACE_FLAG, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Lava_Start_And_End(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{{ }}}", TokenType.LAVA_SHORTHAND_LITERAL_ENTER, TokenType.LAVA_SHORTHAND_LITERAL_EXIT)]
        [InlineData("{# #}", TokenType.LAVA_SHORTHAND_COMMENT_ENTER, TokenType.LAVA_SHORTHAND_COMMENT_EXIT)]
        public void Lexer_Should_Lex_Lava_Shorthand_Start_And_End(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ -}}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% -%}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ -]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData("{{- }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{%- %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[- ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Lava_Start_And_End_With_Whitespace_Trim(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_TRIM_WHITESPACE_FLAG, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{{- }}}", TokenType.LAVA_SHORTHAND_LITERAL_ENTER, TokenType.LAVA_SHORTHAND_LITERAL_EXIT)]
        [InlineData("{#- #}", TokenType.LAVA_SHORTHAND_COMMENT_ENTER, TokenType.LAVA_SHORTHAND_COMMENT_EXIT)]
        public void Lexer_Should_Lex_Lava_Shorthand_Start_And_End_With_Starting_Whitespace_Trim(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_TRIM_WHITESPACE_FLAG, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{{ -}}}", TokenType.LAVA_SHORTHAND_LITERAL_ENTER, TokenType.LAVA_SHORTHAND_LITERAL_EXIT)]
        [InlineData("{# -#}", TokenType.LAVA_SHORTHAND_COMMENT_ENTER, TokenType.LAVA_SHORTHAND_COMMENT_EXIT)]
        public void Lexer_Should_Lex_Lava_Shorthand_Start_And_End_With_Ending_Whitespace_Trim(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_TRIM_WHITESPACE_FLAG, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ }} Test", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% %} Test", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ ]} Test", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Lava_Start_And_End_With_Text(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{{ }}} Test", TokenType.LAVA_SHORTHAND_LITERAL_ENTER, TokenType.LAVA_SHORTHAND_LITERAL_EXIT)]
        [InlineData("{# #} Test", TokenType.LAVA_SHORTHAND_COMMENT_ENTER, TokenType.LAVA_SHORTHAND_COMMENT_EXIT)]
        public void Lexer_Should_Lex_Lava_Shorthand_Start_And_End_With_Text(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test.thing }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test.thing %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test.thing ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier_With_Dot(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.DOT, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test[0] }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test[0] %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test[0] ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData("{{test[0]}}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{%test[0]%}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[test[0]]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier_With_Integer_Indexer(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test[\"test\"] }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test[\"test\"] %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test[\"test\"] ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData("{{test[\"test\"]}}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{%test[\"test\"]%}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[test[\"test\"]]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier_With_String_Indexer(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test[test] }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test[test] %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test[test] ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData("{{test[test]}}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{%test[test]%}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[test[test]]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier_With_Identifier_Indexer(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ test[test].test }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% test[test].test %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ test[test].test ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Identifier_With_Identifier_Indexer_With_Dot(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LEFT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.RIGHT_SQUARE_BRACKET, token.TokenType),
                token => Assert.Equal(TokenType.DOT, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ 0 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% 0 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ 0 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Integer(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ -1 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% -1 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ -1 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Negative_Integer(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.INTEGER, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ 0.123 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% 0.123 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ 0.123 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Decimal(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ -0.123 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% -0.123 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ -0.123 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Negative_Decimal(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ .123 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% .123 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ .123 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Decimal_With_Starting_Dot(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ -.123 }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% -.123 %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ -.123 ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Negative_Decimal_With_Starting_Dot(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.DECIMAL, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ \"test\" }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% \"test\" %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ \"test\" ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Double_Quoted_String(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ \"\" }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% \"\" %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ \"\" ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Empty_Double_Quoted_String(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ 'test' }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% 'test' %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ 'test' ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Single_Quoted_String(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ '' }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% '' %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ '' ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Empty_Single_Quoted_String(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{{ \"te\\\"st\" }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% \"te\\\"st\" %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ \"te\\\"st\" ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData("{{ \"\\\"\" }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData("{% \"\\\"\" %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData("{[ \"\\\"\" ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Double_Quoted_String_With_Escaped_Quote(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData(@"{{ 'te\'st' }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData(@"{% 'te\'st' %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData(@"{[ 'te\'st' ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        [InlineData(@"{{ '\'' }}", TokenType.LAVA_VARIABLE_ENTER, TokenType.LAVA_VARIABLE_EXIT)]
        [InlineData(@"{% '\'' %}", TokenType.LAVA_TAG_ENTER, TokenType.LAVA_TAG_EXIT)]
        [InlineData(@"{[ '\'' ]}", TokenType.LAVA_SHORTCODE_ENTER, TokenType.LAVA_SHORTCODE_EXIT)]
        public void Lexer_Should_Lex_Single_Quoted_String_With_Escaped_Quote(string testString, TokenType expectedStartToken, TokenType expectedEndToken)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(expectedStartToken, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(expectedEndToken, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{[ '{{ test }}' ]}")]
        // [InlineData("{[ \"{{ test }}\" ]}")]
        public void Lexer_Should_Lex_Interpolated_String_1(string testString)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{[ 'hello {{ test }}' ]}")]
        // [InlineData("{[ \"hello {{ test }}\" ]}")]
        public void Lexer_Should_Lex_Interpolated_String_2(string testString)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("{[ 'hello {{ test }} world' ]}")]
        // [InlineData("{[ \"hello {{ test }} world\" ]}")]
        public void Lexer_Should_Lex_Interpolated_String_3(string testString)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.STRING_START, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.IDENTIFIER, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_VARIABLE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.STRING_END, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_SHORTCODE_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("Hello {# comment with {{ lava }} and {% things #} World")]
        public void Lexer_Should_Lex_Shorthand_Comment(string testString)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Collection(
                tokens,
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_SHORTHAND_COMMENT_ENTER, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.LAVA_SHORTHAND_COMMENT_EXIT, token.TokenType),
                token => Assert.Equal(TokenType.RAW_TEXT, token.TokenType),
                token => Assert.Equal(TokenType.EOF, token.TokenType)
            );
        }

        [NiceTheory]
        [InlineData("Hello {{ asdf }} World")]
        [InlineData("Hello {% asdf %} World")]
        [InlineData("Hello {[ asdf ]} World")]
        [InlineData("Hello {[ '{{ test }}' ]} World")]
        [InlineData("Hello {[ '{[ '{{ test }}' ]}' ]} World")]
        [InlineData("Hello {{{ asdf }}} World")]
        [InlineData("Hello {# asdf #} World")]
        public void Lexer_Should_End_With_Empty_Mode_Stack(string testString)
        {
            var lexer = new Lexer(testString);

            var tokens = lexer.ToList();

            Assert.Equal(0, lexer.CurrentLexerModeDepth);

        }

    }
}
