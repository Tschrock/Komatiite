public class CharacterPosition {
    public int Column { get; set; }
    public int Row { get; set; }
    public void BumpRow() {
        this.Column = 0;
        this.Row = this.Row + 1;
    }
}