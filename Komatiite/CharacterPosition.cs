using System;

namespace Komatiite
{

    public class CharacterPosition
    {
        public CharacterPosition(int column, int row, int index)
        {
            Column = column;
            Row = row;
            Index = index;
        }

        public int Column { get; set; }

        public int Row { get; set; }

        public int Index { get; set; }

        private int lastChar = -1;

        public void BumpRow()
        {
            this.Column = 0;
            this.Row = this.Row + 1;
        }

        internal void BumpColumn()
        {
            this.Column = this.Column + 1;
            this.Index = this.Index + 1;
        }

        public void BumpForChar(int c)
        {
            if (lastChar == '\n')
            {
                BumpRow();
            }
            BumpColumn();
            lastChar = c;
        }

        public CharacterPosition Clone()
        {
            return new CharacterPosition(this.Column, this.Row, this.Index);
        }

        public static CharacterPosition Empty = new CharacterPosition(-1, -1, -1);

        public static bool operator ==(CharacterPosition left, CharacterPosition right)
        {
            if (object.ReferenceEquals(null, left))
                return object.ReferenceEquals(null, right);
            return left.Equals(right);
        }

        public static bool operator !=(CharacterPosition left, CharacterPosition right)
        {
            if (object.ReferenceEquals(null, left))
                return !object.ReferenceEquals(null, right);
            return !left.Equals(right);
        }

        public override bool Equals(object other)
        {
            return Equals(other as CharacterPosition);
        }

        public bool Equals(CharacterPosition other)
        {
            return other != null && Column == other.Column && Row == other.Row && Index == other.Index;
        }

        public override int GetHashCode()
        {
            var hashCode = 352033288;
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }

    }

}