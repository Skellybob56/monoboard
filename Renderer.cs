using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

class Renderer : Singleton<Renderer>
{
	public static Renderer Create()
	{ return Register(new Renderer()); }

	// fonts
	readonly Font logoFont;
	const int logoFontSize = 64;
	readonly Font keyFont;
	const int keyFontSize = 44;
	readonly Font playingNoteFont;
	const int playingNoteFontSize = 18;
	readonly Font mappingSelectorFont;
	const int mappingSelectorFontSize = 24;

	private Renderer()
	{
		logoFont = LoadFontEx("assets/CooperMdBT-Regular.ttf", logoFontSize,
			['M', 'O', 'N', 'B', 'A', 'R', 'D'], 7); // MONOBOARD

		keyFont = LoadFontEx("assets/CooperLtBT-Regular.ttf", keyFontSize,
			['A', 'S', 'D', 'F', 'J', 'K', 'L', ';'], 8);

		playingNoteFont = LoadFontEx("assets/AtkinsonHyperlegibleNext-Regular.otf", playingNoteFontSize,
			['A', 'B', 'C', 'D', 'E', 'F', 'G', '#', 'b',
			 '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'], 20);

		mappingSelectorFont = LoadFontEx("assets/AtkinsonHyperlegibleNext-Regular.otf", mappingSelectorFontSize,
			['-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'], 11);
	}

	// inputs
	static readonly Color backgroundColor = Color.Black;
	static readonly Color dividerColor = Color.Gray;
	static readonly Color logoColor = Color.White;
	static readonly Color keyColor = Color.White;
	static readonly Color keyPressedColor = Color.Gray;
	static readonly Color activeOctaveColor = Color.White;
	static readonly Color inactiveOctaveColor = Color.Gray;
	static readonly Color playingNoteColor = Color.Red;

	const int defaultMargin = 12;

	const int noteLineWindowWidth = 64;
	const int baseOctaveHeight = playingNoteFontSize;
	const int noteLineMargin = 16;

	const int logoHeight = logoFontSize;

	const int mappingSelectorHeight = mappingSelectorFontSize;
	const int mappingSelectorMarginY = 6;

	const int keyBoxWidth = 56;
	const int keyBoxHeight = keyBoxWidth;
	const int keyBoxSpacing = 7;
	const int keyPressedSink = 6;

	// maths

	const int logoTextX = noteLineWindowWidth + defaultMargin;
	const int logoTextY = defaultMargin;

	const int logoDividerY = logoTextY + logoHeight;

	const int mappingSelectorX = noteLineWindowWidth + defaultMargin;
	const int mappingSelectorY = logoDividerY + mappingSelectorMarginY;

	const int mappingSelectorDividerY = mappingSelectorY + mappingSelectorHeight + mappingSelectorMarginY;

	const int keySetX = noteLineWindowWidth + defaultMargin;
	const int keySetY = mappingSelectorDividerY + defaultMargin;
	const int keyBoxJump = keyBoxSpacing + keyBoxWidth;

	const int spacebarY = keySetY + keyBoxHeight + defaultMargin;
	const int spacebarWidth = 9*keyBoxWidth + 8*keyBoxSpacing;

	public const int screenWidth = keySetX + spacebarWidth + defaultMargin;
	public const int screenHeight = spacebarY + keyBoxHeight + defaultMargin;

	// todo: vary glsl version by platform (330 for PC, 100 for android and web)
	static Shader roundedSquareShader = LoadShader(null, "assets/shaders/rounded_box.fs");

	public void Render()
	{
		ClearBackground(backgroundColor);

		DrawNoteScaleWindow();

		DrawLine(noteLineWindowWidth, 0, noteLineWindowWidth, screenHeight, dividerColor);

		DrawMainWindow();
	}

	void DrawNoteScaleWindow()
	{
		const int baseOctaveY = defaultMargin;
		const int noteLineY = baseOctaveY + baseOctaveHeight + noteLineMargin;
		const int noteLineLength = screenHeight - noteLineY - noteLineMargin;
		const int octaveHeight = noteLineLength/3;

		const int activeOctaveSeparatorWidth = noteLineWindowWidth - 3*defaultMargin;
		const int inactiveOctaveSeparatorWidth = 3*activeOctaveSeparatorWidth/4;
		const int octaveSeparator1Y = noteLineY + 0*octaveHeight;
		const int octaveSeparator2Y = noteLineY + 1*octaveHeight;
		const int octaveSeparator3Y = noteLineY + 2*octaveHeight;
		const int octaveSeparator4Y = noteLineY + 3*octaveHeight;

		string baseOctaveString = Instrument.BaseOctave.ToString();
		int baseOctaveX = (int)MathF.Round(noteLineWindowWidth/2f - MeasureTextEx(playingNoteFont, baseOctaveString, playingNoteFontSize, 0).X/2f);
		DrawTextEx(playingNoteFont, baseOctaveString, new(baseOctaveX, baseOctaveY), playingNoteFontSize, 0, Color.Red);

		// vertical line
		DrawLine(defaultMargin, octaveSeparator1Y, defaultMargin, octaveSeparator2Y, Instrument.OctaveShift ==  1? activeOctaveColor : inactiveOctaveColor);
		DrawLine(defaultMargin, octaveSeparator2Y, defaultMargin, octaveSeparator3Y, Instrument.OctaveShift ==  0? activeOctaveColor : inactiveOctaveColor);
		DrawLine(defaultMargin, octaveSeparator3Y, defaultMargin, octaveSeparator4Y, Instrument.OctaveShift == -1? activeOctaveColor : inactiveOctaveColor);

		// octave separators
		DrawOctaveSeparator(octaveSeparator1Y, Instrument.OctaveShift ==  1);
		DrawOctaveSeparator(octaveSeparator2Y, Instrument.OctaveShift != -1);
		DrawOctaveSeparator(octaveSeparator3Y, Instrument.OctaveShift !=  1);
		DrawOctaveSeparator(octaveSeparator4Y, Instrument.OctaveShift == -1);

		void DrawOctaveSeparator(int y, bool active)
		{
			DrawLine(defaultMargin, y, defaultMargin + (active? activeOctaveSeparatorWidth : inactiveOctaveSeparatorWidth), y, active? activeOctaveColor : inactiveOctaveColor);
		}

		if (MidiManager.PlayingNote is not null)
		{
			byte playingNote = MidiManager.PlayingNote.Value;

			const int playingNoteArrowWidth = 16;
			const int playingNoteBoxWidth = 3*playingNoteFontSize/2;
			const int playingNoteOutlineThickness = 1;
			const int noteTextOffsetY = 1;

			int playingNoteY = octaveSeparator4Y - (int)MathF.Round((playingNote - (Instrument.BaseOctave-1)*12 - Instrument.RootTone) * octaveHeight/12f);

			// todo: replace aliased solution with AA shader
			DrawTriangleFan(
				[
					new(defaultMargin + playingNoteArrowWidth, playingNoteY - playingNoteFontSize/2),
					new(defaultMargin, playingNoteY),
					new(defaultMargin + playingNoteArrowWidth, playingNoteY + playingNoteFontSize/2),
					new(defaultMargin + playingNoteArrowWidth + playingNoteBoxWidth, playingNoteY + playingNoteFontSize/2),
					new(defaultMargin + playingNoteArrowWidth + playingNoteBoxWidth, playingNoteY - playingNoteFontSize/2),
				],
				5, playingNoteColor);
			DrawTriangleFan(
				[
					new(defaultMargin + playingNoteArrowWidth, playingNoteY - playingNoteFontSize/2 + playingNoteOutlineThickness),
					new(defaultMargin + playingNoteOutlineThickness+0.625f, playingNoteY), // +0.625 is a fudge to reduce aliasing
					new(defaultMargin + playingNoteArrowWidth, playingNoteY + playingNoteFontSize/2 - playingNoteOutlineThickness),
					new(defaultMargin + playingNoteArrowWidth + playingNoteBoxWidth - playingNoteOutlineThickness, playingNoteY + playingNoteFontSize/2 - playingNoteOutlineThickness),
					new(defaultMargin + playingNoteArrowWidth + playingNoteBoxWidth - playingNoteOutlineThickness, playingNoteY - playingNoteFontSize/2 + playingNoteOutlineThickness),
				],
				5, backgroundColor);

			const string noteDisplay = "C C#D D#E F F#G G#A A#B "; // todo: this system needs to be expanded to use flats and maybe even double sharps and flats to best notate the scale. it will also need to be unified with the scale 
			DrawTextEx(playingNoteFont, noteDisplay.Substring(playingNote%12 * 2, 2),
				new(defaultMargin + playingNoteArrowWidth, playingNoteY - playingNoteFontSize/2 + noteTextOffsetY),
				playingNoteFontSize, 0, playingNoteColor);
		}
	}

	void DrawMainWindow()
	{
		DrawTextEx(logoFont, "MONOBOARD", new(logoTextX, logoTextY), logoFontSize, 13, logoColor);

		DrawLine(noteLineWindowWidth, logoDividerY, screenWidth, logoDividerY, dividerColor);

		// todo: render RootTone as a note
		DrawTextEx(mappingSelectorFont, Instrument.RootTone.ToString(), new(mappingSelectorX, mappingSelectorY), mappingSelectorFontSize, 0f, Color.White);
		// todo: add dropdown for changing the mapping

		DrawLine(noteLineWindowWidth, mappingSelectorDividerY, screenWidth, mappingSelectorDividerY, dividerColor);

		DrawKey(keySetX + 0*keyBoxJump, keySetY, Glyph.A, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.A));
		DrawKey(keySetX + 1*keyBoxJump, keySetY, Glyph.S, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.S));
		DrawKey(keySetX + 2*keyBoxJump, keySetY, Glyph.D, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.D));
		DrawKey(keySetX + 3*keyBoxJump, keySetY, Glyph.F, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.F));
		// one key-sized gap here to split ASDF from JKL;
		DrawKey(keySetX + 5*keyBoxJump, keySetY, Glyph.J, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Up));
		DrawKey(keySetX + 6*keyBoxJump, keySetY, Glyph.K, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Down));
		DrawKey(keySetX + 7*keyBoxJump, keySetY, Glyph.L, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Sharp));
		DrawKey(keySetX + 8*keyBoxJump, keySetY, Glyph.Semicolon, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Flat));

		DrawSpace(keySetX, spacebarY, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.ApplyOctave));
	}

	void DrawKey(int x, int y, Glyph glyph, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;

		// todo: draw all boxes first to allow for proper batching
		BeginShaderMode(roundedSquareShader);
		DrawRectangleUV(new(x, y + yOffset, keyBoxWidth, keyBoxHeight), pressed? keyPressedColor : keyColor);
		EndShaderMode();

		DrawGlyph(x, y + yOffset, glyph, backgroundColor);
	}

	static void DrawRectangleUV(Rectangle rec, Color tint)
	{
		Rlgl.Begin(7); // quad mode
		{
			// set color and normal
			Rlgl.Color4f(tint.R/255f, tint.G/255f, tint.B/255f, tint.A/255f);
			Rlgl.Normal3f(0f, 0f, 1f);

			// define all four vertices
			Rlgl.TexCoord2f(0f, 0f); // top left
			Rlgl.Vertex2f(rec.Position.X, rec.Position.Y);

			Rlgl.TexCoord2f(0f, 1f); // bottom left
			Rlgl.Vertex2f(rec.Position.X, rec.Position.Y + rec.Size.Y);

			Rlgl.TexCoord2f(1f, 1f); // bottom right
			Rlgl.Vertex2f(rec.Position.X + rec.Size.X, rec.Position.Y + rec.Size.Y);

			Rlgl.TexCoord2f(1f, 0f); // top right
			Rlgl.Vertex2f(rec.Position.X + rec.Size.X, rec.Position.Y);
		}
		Rlgl.End();
	}

	void DrawGlyph(int x, int y, Glyph glyph, Color color)
	{
		char character = GlyphToChar(glyph);

		Vector2 offset = glyph switch
		{
			Glyph.A => new(12, 10),
			Glyph.S => new(15, 10),
			Glyph.D => new(11, 10),
			Glyph.F => new(14, 10),
			Glyph.J => new(16, 10),
			Glyph.K => new(12, 10),
			Glyph.L => new(13, 10),
			Glyph.Semicolon => new(18, -2),
			_ => throw new ArgumentException("Glyph enum value not recognized.", nameof(glyph))
		};

		DrawTextCodepoint(keyFont, character, new Vector2(x, y) + offset, keyFontSize, color);
	}

	void DrawSpace(int x, int y, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		Color currentKeyColor = pressed? keyPressedColor : keyColor;
		DrawOutlinedBox(x, y + yOffset, spacebarWidth, keyBoxHeight, currentKeyColor, 1, backgroundColor);

		const int spacebarGlyphMarginX = 20;
		const int spacebarGlyphMarginY = 16;
		const int spacebarGlyphWidth = spacebarWidth - (2*spacebarGlyphMarginX);
		const int spacebarGlyphHeight = 18;
		DrawRectangle(x + spacebarGlyphMarginX, y + yOffset + spacebarGlyphMarginY, spacebarGlyphWidth, spacebarGlyphHeight, currentKeyColor);
		DrawRectangle(x + spacebarGlyphMarginX + 1, y + yOffset + spacebarGlyphMarginY, spacebarGlyphWidth-2, spacebarGlyphHeight-1, backgroundColor);
	}

	static void DrawOutlinedBox(int x, int y, int width, int height, Color outlineColor, int outlineThickness, Color fillColor)
	{
		DrawRectangle(x, y, width, height, outlineColor);
		DrawRectangle(x + outlineThickness, y + outlineThickness, width-(2*outlineThickness), height-(2*outlineThickness), fillColor);
	}

	enum Glyph : byte
	{
		A, S, D, F,
		J, K, L, Semicolon
	}

	static char GlyphToChar(Glyph glyph)
	{
		return glyph switch
		{
			Glyph.A => 'A', Glyph.S => 'S', Glyph.D => 'D', Glyph.F => 'F',
			Glyph.J => 'J', Glyph.K => 'K', Glyph.L => 'L', Glyph.Semicolon => ';',
			_ => throw new ArgumentException("Glyph enum value not recognized.", nameof(glyph))
		};
	}
}