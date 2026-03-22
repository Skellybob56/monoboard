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

    static Keymap keymap;
    static Keymap oldKeymap;
    static Keymap differenceKeymap;

    static void UpdateOctave()
    {
        if (differenceKeymap.HasFlag(Keymap.F))
        { octave += keymap.HasFlag(Keymap.F) ? 1 : -1; }
        if (differenceKeymap.HasFlag(Keymap.D))
        { octave -= keymap.HasFlag(Keymap.D) ? 1 : -1; }
    }

    static void UpdateNote()
    {
        // todo: add delay between note changed and update
        if ((differenceKeymap & noteInputMask) != Keymap.None) // if note changed
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
        }
    }


    public static void Update(Keymap newKeymap)
    {
        // todo: assert that noteInputMaps length is 12 (put in init if this class becomes a singleton)

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote();
    }
}
