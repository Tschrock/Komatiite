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

    public void BumpRow()
    {
        this.Column = 0;
        this.Row = this.Row + 1;
    }

    public void BumpForChar(int c)
    {
        if (c != -1)
        {
            if (c == '\n')
            {
                BumpRow();
            }
            Index = Index + 1;
        }
    }

    public CharacterPosition Clone()
    {
        return new CharacterPosition(this.Column, this.Row, this.Index);
    }

    public static CharacterPosition Empty = new CharacterPosition(-1, -1, -1);

}