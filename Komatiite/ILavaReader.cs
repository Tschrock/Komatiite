namespace Komatiite
{

    public interface ILavaReader
    {
        int CurrentCharacter { get; }

        CharacterPosition CurrentPosition { get; }

        int NextCharacter();

        int PeekCharacter(int offset);

        string ToString();

    }

}