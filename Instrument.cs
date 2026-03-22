namespace Monoboard;

static class Instrument
{
    static int octave = 5;

    static readonly bool mirrorMode = false;
    static readonly Keymap noteInputMask = Keymap.J | Keymap.K | Keymap.L | Keymap.C;
    static readonly Keymap[] noteInputMaps = [
        Keymap.J,
        Keymap.J | Keymap.K,
        Keymap.K,
        Keymap.K | Keymap.L,
        Keymap.L,
        Keymap.J | Keymap.L,
        Keymap.J | Keymap.K | Keymap.L,
        Keymap.J | Keymap.K | Keymap.L | Keymap.C,
        Keymap.K | Keymap.L | Keymap.C,
        Keymap.L | Keymap.C,
        Keymap.C,
        Keymap.J | Keymap.C
        ];
    // todo: add octave related flag constants
    static double checkTimeOffset = 0.0625f;

    static Keymap keymap;
    static Keymap oldKeymap;
    static Keymap differenceKeymap;
    static double? changeCheckTime;

    static void UpdateOctave()
    {
        if (differenceKeymap.HasFlag(Keymap.F))
        { octave += keymap.HasFlag(Keymap.F) ? 1 : -1; }
        if (differenceKeymap.HasFlag(Keymap.D))
        { octave -= keymap.HasFlag(Keymap.D) ? 1 : -1; }
    }

    static void UpdateNote(double time)
    {
        if (changeCheckTime.HasValue)
        {
            if (changeCheckTime <= time)
            {
                int noteIndex = noteInputMaps.IndexOf(keymap & noteInputMask);
                if (noteIndex == -1) // rest
                {
                    MidiManager.NoteEvent(octave, null);
                }
                else
                {
                    MidiManager.NoteEvent(octave, noteIndex);
                }
                changeCheckTime = null;
            }
        }
        else
        {
            if ((differenceKeymap & noteInputMask) != Keymap.None)
            { changeCheckTime = time + checkTimeOffset; } // if note changed
        }
    }


    public static void Update(Keymap newKeymap, double time)
    {
        // todo: assert that noteInputMaps length is 12 (put in init if this class becomes a singleton)

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote(time);
    }
}
