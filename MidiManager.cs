using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Monoboard;

static class MidiManager
{
    static readonly SevenBitNumber noteVelocity = (SevenBitNumber)100;
    static readonly OutputDevice output = OutputDevice.GetByName("monoboard");

    static SevenBitNumber? playingNoteNumber;
    
    public static void NoteEvent(int octave, int? note)
    {
        // todo: add testing to ensure octave and note are within legal ranges
        if (!note.HasValue)
        {
            if (!playingNoteNumber.HasValue)
            { throw new Exception("Attempted to stop playing a note while no note was playing"); }
            output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0));
            return;
        }

        if (playingNoteNumber.HasValue)
        { output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0)); }

        SevenBitNumber noteNumber = (SevenBitNumber)(byte)(octave * 12 + note.Value);
        output.SendEvent(new NoteOnEvent(noteNumber, noteVelocity));
        playingNoteNumber = noteNumber;
    }

    public static void Dispose()
    {
        output.Dispose();
    }
}
