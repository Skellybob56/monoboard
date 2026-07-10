using static Monoboard.KeymapUtil;

namespace Monoboard;

static class FileReader
{
    const string combinationsFolder = "assets/patterns/";
    const string combinationsExtention = ".kmap";
    const string notesFolder = "assets/patterns/";
    const string notesExtention = ".nmap";

    public static Keymap[] GetCombinations(string filename)
    {
        // todo: modernise this to not assume that the note input mask is 1111 0000
        string path = combinationsFolder + filename + combinationsExtention;
        StreamReader streamReader = new StreamReader(path);

        int position = 0;
        List<Keymap> combinations = [];
        Keymap? currentKeymap = null;
        for (char character = (char)streamReader.Read(); !streamReader.EndOfStream; character = (char)streamReader.Read())
        {
            if (currentKeymap.HasValue) // keep reading current run
            {
                position++;
                if (position > 3) { throw new Exception($"Combination file at '{path}' contains a keymap that is more than 4 digits in length."); }
                if (character != '.' && character != '!') { throw new Exception($"Combination file at '{path}' contains a keymap that is less than 4 digits in length."); }

                if (position == 3) // last digit in run
                {
                    // add last digit
                    if (character == '!') { currentKeymap |= Keymap.F; }

                    // append to array and clear currentKeymap
                    combinations.Add(currentKeymap.Value);
                    currentKeymap = null;
                    continue;
                }
                if (character == '!')
                {
                    if (position == 2) { currentKeymap |= Keymap.D; }
                    else if (position == 1) { currentKeymap |= Keymap.S; }
                }
            }
            else if (character == '.' || character == '!')
            {
                currentKeymap = character == '!' ? Keymap.A : Keymap.None;
                position = 0;
            }
        }

        // todo: check if there are any repetitions and throw an exception if so

        return combinations.ToArray();
    }

    public static sbyte[] GetNotes(string filename, int rootNote, int correctLength)
    {
        const string numbers = "0123456789";

        string path = notesFolder + filename + notesExtention;
        StreamReader streamReader = new StreamReader(path);

        List<sbyte> notes = [];
        int? currentNote = null;
        bool negative = false;
        for (char character; !streamReader.EndOfStream; )
        {
            character = (char)streamReader.Read();

            if (currentNote.HasValue)
            {
                if (numbers.Contains(character)) // new digit
                {
                    currentNote *= 10;
                    currentNote += (sbyte)(character - numbers[0]);
                }
                else // number ended
                {
                    notes.Add((sbyte)((negative? -currentNote.Value : currentNote.Value) + rootNote));
                    currentNote = null;
                    negative = false;
                }
            }
            else if (character == '-')
            {
                negative = true;
            }
            else if (numbers.Contains(character))
            {
                currentNote = (sbyte)(character - numbers[0]);
            }
        }
        if (currentNote.HasValue) // number ended at eof
        {
            notes.Add((sbyte)((negative ? -currentNote.Value : currentNote.Value) + rootNote));
            currentNote = null;
            negative = false;
        }

        if (notes.Count != correctLength)
        { throw new Exception("The number of notes loaded is not equal to the number of combinations."); }

        return notes.ToArray();
    }
}
