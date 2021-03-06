namespace Komatiite
{

    public enum TokenType
    {
        RAW_TEXT,
        LAVA_VARIABLE_ENTER,
        LAVA_VARIABLE_EXIT,
        LAVA_TAG_ENTER,
        LAVA_TAG_EXIT,
        LAVA_SHORTCODE_ENTER,
        LAVA_SHORTCODE_EXIT,
        LAVA_SHORTHAND_LITERAL_ENTER,
        LAVA_SHORTHAND_LITERAL_EXIT,
        LAVA_SHORTHAND_COMMENT_ENTER,
        LAVA_SHORTHAND_COMMENT_EXIT,
        STRING_START,
        STRING_END,
        LAVA_TRIM_WHITESPACE_FLAG,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        ASSIGNMENT,
        EQUALS,
        NOT_EQUAL,
        GREATER_THAN,
        LESS_THAN,
        GREATER_THAN_OR_EQUAL,
        LESS_THAN_OR_EQUAL,
        COMMA,
        LEFT_PARENTHESES,
        RIGHT_PARENTHESES,
        COLON,
        PIPE,
        INTEGER,
        DECIMAL,
        IDENTIFIER,
        RANGE,
        DOT,
        EOF

    }

}
