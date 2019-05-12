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
            EndPosition = CharacterPosition.Empty;
        }
        public Token(TokenType type, CharacterPosition startPosition)
        {
            TokenType = type;
            StartPosition = startPosition;
            EndPosition = startPosition;
        }
        public Token(TokenType type, CharacterPosition startPosition, CharacterPosition endPosition)
        {
            TokenType = type;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public TokenType TokenType { get; set; }
        public CharacterPosition StartPosition { get; set; }
        public CharacterPosition EndPosition { get; set; }
        public int Length
        {
            get
            {
                return EndPosition.Index - StartPosition.Index;
            }
        }

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
            return other != null && TokenType == other.TokenType && StartPosition == other.StartPosition && EndPosition == other.EndPosition;
        }

        public override int GetHashCode()
        {
            var hashCode = 352033288;
            hashCode = hashCode * -1521134295 + TokenType.GetHashCode();
            hashCode = hashCode * -1521134295 + StartPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + EndPosition.GetHashCode();
            return hashCode;
        }

    }

}