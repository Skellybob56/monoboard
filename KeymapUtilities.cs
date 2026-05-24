namespace Monoboard;

static class KeymapUtilities
{
    public static string KeymapToString(this Keymap keymap)
    {
        int index = 0;
        char[] output = new char[8];
        for (byte i = 1; i != 0; i <<= 1)
        {
            output[index++] = keymap.HasFlag((Keymap)i) ? '!' : '.';
        }

        return new string(output);
    }
}
