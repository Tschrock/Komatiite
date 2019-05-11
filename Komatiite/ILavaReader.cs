public interface ILavaReader
{
    int CurrentCharacter { get; }

    int NextCharacter();

    int PeekCharacter(int offset);

    string ToString();

}