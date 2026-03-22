using System.Security.Cryptography;

namespace Monoboard;

static class Instrument
{
    static int octave = 5;

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

    static Keymap oldKeymap;

    public static void Update(Keymap keymap)
    {
        Keymap changedButtons = keymap ^ oldKeymap;
        // todo: assert that noteInputMaps length is 12

        // todo: write code to manage octave

        // manage note
        // todo: add delay between note changed and update
        if ((changedButtons & noteInputMask) != Keymap.None) // if note changed
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

        // old variables
        oldKeymap = keymap;
    }
}
