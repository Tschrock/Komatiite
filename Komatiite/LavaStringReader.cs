using System.IO;
using System.Text;

namespace Komatiite
{

    public class LavaStringReader : ILavaReader
    {

        private string inputString;

        private int currentIndex = 0;

        private CharacterPosition currentPosition = new CharacterPosition(0, 0, 0);

        public LavaStringReader(string input)
        {
            inputString = input;
        }

        public int CurrentCharacter => currentIndex < inputString.Length ? inputString[currentIndex] : -1;

        public CharacterPosition CurrentPosition => currentPosition;

        public int NextCharacter()
        {
            currentIndex = currentIndex + 1;

            if (currentIndex >= inputString.Length)
            {
                currentPosition.BumpForChar(-1);
                return -1;
            }

            var c = inputString[currentIndex];

            currentPosition.BumpForChar(c);

            return c;
        }

        public int PeekCharacter(int offset)
        {
            var peekIndex = currentIndex + offset;

            if (peekIndex >= inputString.Length) return -1;

            return inputString[peekIndex];
        }

        public override string ToString()
        {
            return inputString;
        }

    }

}
