using System.IO;
using System.Text;

public class LavaStringReader : ILavaReader {

    private string inputString;

    private int currentIndex = 0;

    public LavaStringReader(string input) {
        inputString = input;
    }

    public int CurrentCharacter => inputString[currentIndex];

    public int NextCharacter()
    {
        currentIndex = currentIndex + 1;

        if(currentIndex >= inputString.Length) return -1;

        return inputString[currentIndex];
    }

    public int PeekCharacter(int offset)
    {
        var peekIndex = currentIndex + offset;

        if(peekIndex >= inputString.Length) return -1;

        return inputString[peekIndex];
    }

    public override string ToString() {
        return inputString;
    }

}