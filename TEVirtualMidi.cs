using System.Runtime.InteropServices;

namespace TEVirtualMidiBindingsCs;

static partial class TEVirtualMidi
{
	[Flags]
	public enum VmFlags : uint
	{
		ParseRX = 0b0001,
		ParseTX = 0b0010,
		InstantiateRX = 0b0100,
		InstantiateTX = 0b1000,
		InstantiateBoth = 0b1100
	}
	
	[LibraryImport("teVirtualMIDI64.dll", EntryPoint = "virtualMIDISendData")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool virtualMIDISendData(nint lpvmMidiPort, [In] byte[] midiDataBytes, uint length);

	[LibraryImport("teVirtualMIDI64.dll", EntryPoint = "virtualMIDICreatePortEx2", StringMarshalling = StringMarshalling.Utf16)]
	public static partial nint virtualMIDICreatePortEx2(string portName, nint callback, nint callbackInstance, uint maxSysexLength, uint flags);

	[LibraryImport("teVirtualMIDI64.dll", EntryPoint = "virtualMIDIClosePort")]
	public static partial void virtualMIDIClosePort(nint lpvmMidiPort);
}
