namespace Monoboard;

static class Instrument
{
    static int octave = 5;

    const bool mirrorMode = false;
    const Keymap noteInputMask = Keymap.J | Keymap.K | Keymap.L | Keymap.C;
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
        Keymap.J | Keymap.C,
        Keymap.J | Keymap.K | Keymap.C
        ];
    const Keymap octaveShiftUpKey = Keymap.F;
    const Keymap octaveShiftDownKey = Keymap.D;
    const Keymap octaveApplyKey = Keymap.S;
    const double checkTimeOffset = 0.0625f;

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
        // todo: assert that noteInputMaps length is 13 (put in init if this class becomes a singleton)

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote(time);
    }
}
