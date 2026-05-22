using TEVirtualMidiBindingsCs;
using System.Runtime.InteropServices;

namespace Monoboard;

static class MidiPortHandler
{
	public static bool Active => LpvmMidiPort != nint.Zero;
	public static nint LpvmMidiPort { get; private set; } = nint.Zero;

	public static void Initialize(string portName)
	{
		if (Active)
		{
			throw new InvalidOperationException($"{nameof(MidiPortHandler)} is already initialised.");
		}

		// create midi port with virtualMIDICreatePortEx2 from teVirtualMIDI64.dll
		LpvmMidiPort = TEVirtualMidi.virtualMIDICreatePortEx2(portName, nint.Zero, nint.Zero, 65535, (uint)TEVirtualMidi.VmFlags.InstantiateTX);
		if (!Active)
		{ throw new Exception("Something went wrong with creating the virtual midi port"); }
	}

    public static void Dispose()
	{
		if (!Active)
		{
			throw new InvalidOperationException($"{nameof(MidiPortHandler)} is not initialised.");
		}
		
		TEVirtualMidi.virtualMIDIClosePort(LpvmMidiPort);
		LpvmMidiPort = nint.Zero;
	}
}
