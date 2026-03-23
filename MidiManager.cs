using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Monoboard;

static class MidiManager
{
    static readonly SevenBitNumber noteVelocity = (SevenBitNumber)100;
    static readonly OutputDevice output = OutputDevice.GetByName("monoboard");

    static SevenBitNumber? playingNoteNumber;
    
    public static void NoteEvent((int octave, int note)? noteData)
    {
        // todo: add testing to ensure octave and note are within legal ranges
        if (!noteData.HasValue)
        {
            if (!playingNoteNumber.HasValue)
            { return; } // Attempted to stop playing a note while no note was playing
            output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0));
            playingNoteNumber = null;
            return;
        }

        if (playingNoteNumber.HasValue)
        { output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0)); }

        SevenBitNumber noteNumber = (SevenBitNumber)(byte)(noteData.Value.octave * 12 + noteData.Value.note);
        output.SendEvent(new NoteOnEvent(noteNumber, noteVelocity));
        playingNoteNumber = noteNumber;
    }

    public static void Dispose()
    {
        output.Dispose();
    }
}
