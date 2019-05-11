using System.IO;
using System.Text;

public class LavaStreamReader : ILavaReader
{

    private Stream inputStream;

    private StreamReader inputReader;

    private StringBuilder sourceStringBuilder;

    private string sourceString;

    private int currentCharacter = -1;

    private int[] peekBuffer = new int[8];

    private int peekStartIndex = 0;

    private int peekLength = 0;

    public LavaStreamReader(Stream inputStream)
    {
        this.inputStream = inputStream;
        this.inputReader = new StreamReader(inputStream);
    }

    public int CurrentCharacter => currentCharacter;

    public int NextCharacter()
    {
        // Check if anything's been peek buffered
        if (peekLength > 0)
        {
            // If so, use from the buffer
            peekStartIndex = (peekStartIndex + 1) & 7;
            peekLength = peekLength - 1;
            return currentCharacter = peekBuffer[peekStartIndex];
        }
        else
        {
            // If not, just read the next char
            currentCharacter = inputReader.Read();
            RecordCharacter(currentCharacter);
            return currentCharacter;
        }
    }

    public int PeekCharacter(int offset)
    {
        // Buffer more characters if needed
        while (peekLength < offset)
        {
            var writeIndex = (peekStartIndex + peekLength) & 7;
            peekBuffer[writeIndex] = inputReader.Read();
            RecordCharacter(peekBuffer[writeIndex]);
            peekLength = peekLength + 1;
        }

        // Return the buffered value
        var peakIndex = (peekStartIndex + offset) & 7;
        return peekBuffer[peakIndex];
    }

    public override string ToString() {
        if(sourceString != null) return sourceString;
        else return sourceString = sourceStringBuilder.ToString();
    }

    private void RecordCharacter(int c) {
        if(c != -1) {
           sourceString = null;
           sourceStringBuilder.Append(c); 
        }
    }

}