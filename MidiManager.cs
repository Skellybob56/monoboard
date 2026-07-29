using System.Diagnostics;
using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
	public static MidiManager Create()
	{ return Register(new MidiManager()); }

	const string midiDeviceName = "monoboard";
	const byte noteVelocity = 80; // limited to a 7 bit integer (80 is considered mf in musical volume)
	const bool startNoteBeforeEndNote = true; // when swapping between two notes, this will start the next note before ending the last which can prevent monophonic clicking

	public static byte? PlayingNote { get; private set; } = null;

	private MidiManager()
	{
		MidiPortHandler.Initialize(midiDeviceName);
	}

	public void NoteEvent((int octave, int note)? noteData)
	{
		Program.ScheduleGraphicalUpdate(); // to update currently playing note display

		if (noteData is null)
		{ NoteOffEvent(); return; } // null noteData: treat as a rest

		int newNoteInt = noteData.Value.octave * 12 + noteData.Value.note;
		if (newNoteInt < 0x00 || newNoteInt > 0x7f)
		{ NoteOffEvent(); return; } // note out of range: treat as a rest

		NoteOnEvent((byte)newNoteInt);
	}

	void NoteOnEvent(byte newNote)
	{
		Debug.Assert(newNote < 0x7F);

		if (PlayingNote == newNote) { return; } // note is already playing

		if (!startNoteBeforeEndNote && PlayingNote is not null)
		{ TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, PlayingNote.Value, 0x00], 3); }

		TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x90, newNote, noteVelocity], 3);

		if (startNoteBeforeEndNote && PlayingNote is not null)
		{ TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, PlayingNote.Value, 0x00], 3); }

		PlayingNote = newNote;
	}

	void NoteOffEvent()
	{
		if (PlayingNote is null)
		{ return; } // attempted to stop playing a note while no note was playing

		TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, PlayingNote.Value, 0x00], 3);
		PlayingNote = null;
	}

	protected override void Dispose()
	{
		MidiPortHandler.Dispose();
	}
}
