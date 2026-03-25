namespace Monoboard;

class Instrument : Singleton<Instrument>
{
    public static Instrument Create(MidiManager midiManager)
    { return Register(new Instrument(midiManager)); }

    const bool mirrorMode = true;
    const Keymap noteInputMask = Keymap.J | Keymap.K | Keymap.L | Keymap.C;
    const Keymap octaveShiftUpKey = Keymap.F;
    const Keymap octaveShiftDownKey = Keymap.D;
    const Keymap octaveApplyKey = Keymap.S;
    const double checkTimeOffset = 3d / 32d; // in seconds

    readonly MidiManager midiManager;

    int octave = 5;
    Keymap keymap;
    Keymap oldKeymap;
    Keymap differenceKeymap;
    double? changeCheckTime;
    bool octaveShiftedUp = false;
    bool octaveShiftedDown = false;

    Keymap[] combinations;
    sbyte[] notes;

    void InitCombinationsAndScale(string combinationsFilename, string notesFilename, int rootNote)
    {
        combinations = FileReader.GetCombinations(combinationsFilename);
        notes = FileReader.GetNotes(notesFilename, rootNote, combinations.Length);
    }

    private Instrument(MidiManager midiManager)
    {
        this.midiManager = midiManager;
        InitCombinationsAndScale("Diatonic LC03", "Major 8", -5);
    }

    void UpdateOctave()
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

    void UpdateNote(double time)
    {
        if (changeCheckTime.HasValue)
        {
            if (changeCheckTime <= time)
            {
                int noteIndex = combinations.IndexOf(keymap & noteInputMask);
                if (noteIndex == -1) // rest
                {
                    midiManager.NoteEvent(null);
                }
                else
                {
                    midiManager.NoteEvent((octave, notes[noteIndex]));
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


    public void Update(Keymap newKeymap, double time)
    {
        // todo: assert that the length of combinations is the same as the length of notes

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote(time);
    }
}
