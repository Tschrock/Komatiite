using System.IO;
using System.Text;

namespace Komatiite
{

    public class LavaStreamReader : ILavaReader
    {

        private Stream inputStream;

        private StreamReader inputReader;

        private StringBuilder sourceStringBuilder = new StringBuilder();

        private string sourceString;

        private int currentCharacter = -1;

        private CharacterPosition currentPosition = new CharacterPosition(0, 0, 0);

        private int[] readBuffer = new int[8];

        private int readBufferStartIndex = 0;

        private int readBufferLength = 0;

        public LavaStreamReader(Stream inputStream)
        {
            this.inputStream = inputStream;
            this.inputReader = new StreamReader(inputStream);
        }

        public int CurrentCharacter => currentCharacter;

        public CharacterPosition CurrentPosition => currentPosition;

        public int NextCharacter()
        {

            // Check if anything's been buffered ahead
            if (readBufferLength > 1)
            {

                // Move the start index forward
                readBufferStartIndex = (readBufferStartIndex + 1) & 7;

                // Decrement the length
                readBufferLength = readBufferLength - 1;

                // Read the buffered character
                currentCharacter = readBuffer[readBufferStartIndex];

            }
            else
            {
                // Read the next character
                currentCharacter = inputReader.Read();

                // Save the character into the read buffer so we can Peek(0) it
                readBuffer[readBufferStartIndex] = currentCharacter;
            }

            // Record the character
            RecordCharacter(currentCharacter);

            // Bump the character position
            currentPosition.BumpForChar(currentCharacter);

            // Return the new current character;
            return currentCharacter;

        }

        public int PeekCharacter(int offset)
        {

            // Buffer more characters if needed
            while (readBufferLength <= offset)
            {

                // Calculate the next write position
                var writeIndex = (readBufferStartIndex + readBufferLength) & 7;

                // Read the next character
                readBuffer[writeIndex] = inputReader.Read();

                // Increment the length
                readBufferLength = readBufferLength + 1;

            }

            // Calculate the read index
            var readIndex = (readBufferStartIndex + offset) & 7;

            // Return the buffered value
            return readBuffer[readIndex];

        }

        public override string ToString()
        {
            if (sourceString != null) return sourceString;
            else return sourceString = sourceStringBuilder.ToString();
        }

        private void RecordCharacter(int c)
        {
            if (c != -1)
            {
                sourceString = null;
                sourceStringBuilder.Append(c);
            }
        }

    }

}