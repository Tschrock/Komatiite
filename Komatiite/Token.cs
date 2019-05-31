using System;
using System.Text;

namespace Komatiite
{

    public class Token : IEquatable<Token>
    {
        public Token(TokenType type)
        {
            TokenType = type;
            StartPosition = CharacterPosition.Empty;
            Length = 0;
        }
        public Token(TokenType type, CharacterPosition startPosition)
        {
            TokenType = type;
            StartPosition = startPosition;
            Length = 0;
        }
        public Token(TokenType type, CharacterPosition startPosition, int length)
        {
            TokenType = type;
            StartPosition = startPosition;
            Length = length;
        }

        public TokenType TokenType { get; set; }
        public CharacterPosition StartPosition { get; set; }
        public int Length { get; set; }

        public static bool operator ==(Token left, Token right)
        {
            if (object.ReferenceEquals(null, left))
                return object.ReferenceEquals(null, right);
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            if (object.ReferenceEquals(null, left))
                return !object.ReferenceEquals(null, right);
            return !left.Equals(right);
        }

        public override bool Equals(object other)
        {
            return Equals(other as Token);
        }

        public bool Equals(Token other)
        {
            return other != null && TokenType == other.TokenType && StartPosition == other.StartPosition && Length == other.Length;
        }

        public override int GetHashCode()
        {
            var hashCode = 352033288;
            hashCode = hashCode * -1521134295 + TokenType.GetHashCode();
            hashCode = hashCode * -1521134295 + StartPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }

    }

}