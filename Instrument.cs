namespace Monoboard;

static class Instrument
{
    const bool mirrorMode = true;
    const Keymap noteInputMask = Keymap.J | Keymap.K | Keymap.L | Keymap.C;

    static Keymap[] combinations = [
        Keymap.J,
        Keymap.J | Keymap.L,
        Keymap.J | Keymap.K | Keymap.L,
        Keymap.J | Keymap.K,
        Keymap.K,
        Keymap.K | Keymap.L,
        Keymap.J | Keymap.K | Keymap.L | Keymap.C,
        Keymap.J | Keymap.L | Keymap.C
        ];
    static sbyte[] notes = [0, 2, 4, 5, 7, 9, 11, 12];

    const Keymap octaveShiftUpKey = Keymap.F;
    const Keymap octaveShiftDownKey = Keymap.D;
    const Keymap octaveApplyKey = Keymap.S;
    const double checkTimeOffset = 3d / 32d; // in seconds
    const int rootNote = -5;

    static int octave = 5;
    static Keymap keymap;
    static Keymap oldKeymap;
    static Keymap differenceKeymap;
    static double? changeCheckTime;
    static bool octaveShiftedUp = false;
    static bool octaveShiftedDown = false;

    static void UpdateOctave()
    {
        if (differenceKeymap.HasFlag(octaveShiftUpKey))
        {
            if (keymap.HasFlag(octaveShiftUpKey) && !octaveShiftedUp)
            {
                octave += 1;
                octaveShiftedUp = true;
            }
            else if (octaveShiftedUp)
            {
                octave -= 1;
                octaveShiftedUp = false;
            }
        }
        if (differenceKeymap.HasFlag(octaveShiftDownKey))
        {
            if (keymap.HasFlag(octaveShiftDownKey) && !octaveShiftedDown)
            {
                octave -= 1;
                octaveShiftedDown = true;
            }
            else if (octaveShiftedDown)
            {
                octave += 1;
                octaveShiftedDown = false;
            }
        }
        if (differenceKeymap.HasFlag(octaveApplyKey) && keymap.HasFlag(octaveApplyKey))
        {
            octaveShiftedUp = false;
            octaveShiftedDown = false;
        }
    }

    static void UpdateNote(double time)
    {
        if (changeCheckTime.HasValue)
        {
            if (changeCheckTime <= time)
            {
                int noteIndex = combinations.IndexOf(keymap & noteInputMask);
                if (noteIndex == -1) // rest
                {
                    MidiManager.NoteEvent(null);
                }
                else
                {
                    MidiManager.NoteEvent((octave, notes[noteIndex] + rootNote));
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
        // todo: make this class a singleton and add functions to explicitly update combinations and notes arrays to allow for things like root note to be encoded in them
        // todo: assert that the length of combinations is the same as the length of notes

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote(time);
    }
}
