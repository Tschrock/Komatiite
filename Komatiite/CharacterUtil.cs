using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Komatiite
{
    public static class CharacterUtil
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(int c)
        {
            return c >= '0' && c <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentifierStart(int c)
        {
            var cat = Char.GetUnicodeCategory((char)c);

            return  c == '_'
                || cat == UnicodeCategory.LowercaseLetter
                || cat == UnicodeCategory.TitlecaseLetter
                || cat == UnicodeCategory.ModifierLetter
                || cat == UnicodeCategory.OtherLetter
                || cat == UnicodeCategory.LetterNumber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentifierPart(int c)
        {
            var cat = Char.GetUnicodeCategory((char)c);

            return  cat == UnicodeCategory.UppercaseLetter
                || cat == UnicodeCategory.LowercaseLetter
                || cat == UnicodeCategory.TitlecaseLetter
                || cat == UnicodeCategory.ModifierLetter
                || cat == UnicodeCategory.OtherLetter
                || cat == UnicodeCategory.LetterNumber
                || cat == UnicodeCategory.DecimalDigitNumber
                || cat == UnicodeCategory.ConnectorPunctuation
                || cat == UnicodeCategory.NonSpacingMark
                || cat == UnicodeCategory.SpacingCombiningMark
                || cat == UnicodeCategory.Format;
        }
    }
}