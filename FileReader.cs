namespace Monoboard;

static class FileReader
{
    public static Keymap[] GetCombinations(string filename)
    {
        string path = $"assets/patterns/{filename}.kmap";
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
                    if (character == '!') { currentKeymap |= Keymap.C; }

                    // append to array and clear currentKeymap
                    combinations.Add(currentKeymap.Value);
                    currentKeymap = null;
                    continue;
                }
                if (character == '!')
                {
                    if (position == 2) { currentKeymap |= Keymap.L; }
                    else if (position == 1) { currentKeymap |= Keymap.K; }
                }
            }
            else if (character == '.' || character == '!')
            {
                currentKeymap = character == '!' ? Keymap.J : Keymap.None;
                position = 0;
            }
        }

        // todo: check if there are any repetitions and throw an exception if so

        return combinations.ToArray();
    }
}
