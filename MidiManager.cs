using System.Diagnostics;
using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager() : Singleton<MidiManager>
{
	MidiPortHandler midiPortHandler = new(midiDeviceName);

	const string midiDeviceName = "monoboard";
	const byte noteVelocity = 80; // limited to a 7 bit integer (80 is considered mf in musical volume)
	static readonly bool startNoteBeforeEndNote = true; // when swapping between two notes, this will start the next note before ending the last which can prevent monophonic clicking

	public static byte? PlayingNote { get; private set; } = null;

	// returns false when the note it out of midi range
	public bool NoteEvent((int octave, int note)? noteData)
	{
		Program.ScheduleGraphicalUpdate(); // to update currently playing note display

		if (noteData is null)
		{ NoteOffEvent(); return true; } // null noteData is a rest

		// add 1 to the octave because midi 0 is actually C-1 rather than C0
		int newNoteInt = (noteData.Value.octave + 1) * 12 + noteData.Value.note;
		if (newNoteInt < 0x00 || newNoteInt > 0x7f)
		{ NoteOffEvent(); return false; } // note out of range: treat as a rest and fail

		NoteOnEvent((byte)newNoteInt);
		return true;
	}

	void NoteOnEvent(byte newNote)
	{
		Debug.Assert(newNote <= 0x7F);

		if (PlayingNote == newNote) { return; } // note is already playing

		void SendOff() { if (PlayingNote is not null) TEVirtualMidi.virtualMIDISendData(midiPortHandler.LpvmMidiPort, [0x80, PlayingNote.Value, 0x00], 3); }
		void SendOn() { TEVirtualMidi.virtualMIDISendData(midiPortHandler.LpvmMidiPort, [0x90, newNote, noteVelocity], 3); }

		if (startNoteBeforeEndNote) { SendOn(); SendOff(); } else { SendOff(); SendOn(); }

		PlayingNote = newNote;
	}

	void NoteOffEvent()
	{
		if (PlayingNote is null)
		{ return; } // attempted to stop playing a note while no note was playing

		TEVirtualMidi.virtualMIDISendData(midiPortHandler.LpvmMidiPort, [0x80, PlayingNote.Value, 0x00], 3);
		PlayingNote = null;
	}

	protected override void OnDisposal()
	{
		midiPortHandler.Dispose();
	}
}
