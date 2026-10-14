using System.Collections.Immutable;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

class Renderer() : Singleton<Renderer>
{
	// fonts
	const int logoFontSize = 64;
	readonly Font logoFont = LoadFontEx("assets/CooperMdBT-Regular.ttf", logoFontSize,
		['M', 'O', 'N', 'B', 'A', 'R', 'D'], 7); // MONOBOARD

	const int keyFontSize = 44;
	readonly Font keyFont = LoadFontEx("assets/CooperLtBT-Regular.ttf", keyFontSize,
		['A', 'S', 'D', 'F', 'J', 'K', 'L', ';'], 8);

	const int playingNoteFontSize = 18;
	readonly Font playingNoteFont = LoadFontEx("assets/AtkinsonHyperlegibleNext-Regular.otf", playingNoteFontSize,
		['A', 'B', 'C', 'D', 'E', 'F', 'G', '#', 'b',
		 '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'], 20);

	const int mappingSelectorFontSize = 24;
	readonly Font mappingSelectorFont = LoadFontEx("assets/AtkinsonHyperlegibleNext-Regular.otf", mappingSelectorFontSize,
		['-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'], 11);

	static Shader roundedBoxShader = LoadShader("assets/shaders/rounded_box.vs", "assets/shaders/rounded_box.fs");

	// inputs
	static readonly Color backgroundColor = new(40, 32, 32);
	static readonly Color dividerColor = Color.Gray;
	static readonly Color logoColor = Color.White;
	static readonly Color activeOctaveColor = Color.White;
	static readonly Color inactiveOctaveColor = Color.Gray;
	static readonly Color playingNoteColor = Color.Red;
	static readonly Color keyColor = new(213, 208, 180);
	static readonly ImmutableArray<Color> keyPressedColors = [
		new(144,  58,  59), new( 59,  87, 115), new( 56,  92,  73), new(166, 109,  56), // ASDF
		new(117, 114,  98), new(117, 114,  98), new(111, 118, 119), new(111, 118, 119), // JKL;
		new(117, 114,  98)
	];

	const float roundedBoxRadius = 17.5f;

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
	const int keySetWidth = keyBoxJump*8 + keyBoxWidth;

	const int spacebarY = keySetY + keyBoxHeight + defaultMargin;
	const int spacebarX = keySetX + keyBoxJump;
	const int spacebarWidth = 6*keyBoxJump + keyBoxWidth;

	public const int screenWidth = keySetX + keySetWidth + defaultMargin;
	public const int screenHeight = spacebarY + keyBoxHeight + defaultMargin;

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

			int rootNote = Instrument.BaseOctave*12 + Instrument.RootTone;
			int playingNoteY = octaveSeparator4Y - (int)MathF.Round((playingNote - rootNote) * octaveHeight/12f);

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

		BeginShaderMode(roundedBoxShader);
		{
			DrawSpace(spacebarX, spacebarY, Controller.CurrentKeymap.HasFlag(Keymap.ApplyOctave)); // draw space first to batch it with the key boxes (which are drawn before the glyphs)

			DrawKeyBoxes();
		}

		EndShaderMode();

		DrawKeyGlyphs();
	}

	void DrawKeyBoxes()
	{
		for (int i = 0; i < 8; i++)
		{
			bool pressed = Controller.CurrentKeymap.HasFlag(keymapNoteModiferKeys[i]);
			int slot = i >= 4 ? i + 1 : i; // skip the 5th slot to split ASDF from JKL;
			int x = keySetX + slot * keyBoxJump;

			int yOffset = pressed? keyPressedSink : 0;

			DrawRoundedRectangle(new(x, keySetY + yOffset, keyBoxWidth, keyBoxHeight), pressed? keyPressedColors[i] : keyColor, roundedBoxRadius);
		}
	}

	void DrawKeyGlyphs()
	{
		for (int i = 0; i < 8; i++)
		{
			bool pressed = Controller.CurrentKeymap.HasFlag(keymapNoteModiferKeys[i]);
			int slot = i >= 4 ? i + 1 : i; // skip the 5th slot to split ASDF from JKL;
			int x = keySetX + slot * keyBoxJump;

			int yOffset = pressed? keyPressedSink : 0;

			DrawGlyph(x, keySetY + yOffset, (Glyph)i, backgroundColor);
		}
	}

	// caller must BeginShaderMode(roundedSquareShader)
	static void DrawRoundedRectangle(Rectangle rec, Color tint, float radius)
	{
		// todo: implement window resizing and clean everything like this up
		Vector2 recRadii = rec.Size/2f;

		DrawRectUv(rec,
			new(-recRadii.X, -recRadii.Y),
			new(-recRadii.X, +recRadii.Y),
			new(+recRadii.X, +recRadii.Y),
			new(+recRadii.X, -recRadii.Y),
			tint, new(recRadii.X - radius, recRadii.Y - radius, radius));
	}

	// caller must BeginShaderMode(roundedSquareShader)
	static void DrawRoundedBaseRectangle(Rectangle rec, Color tint, float radius)
	{
		Vector2 recRadii = new(rec.Size.X/2f, rec.Size.Y); // y size is doubled to pretend that the rounded rectangle is twice as large

		DrawRectUv(rec,
			new(-recRadii.X, 0), // normally negative y values are 0 here to split the double-height rectangle in half
			new(-recRadii.X, +recRadii.Y),
			new(+recRadii.X, +recRadii.Y),
			new(+recRadii.X, 0),
			tint, new(recRadii.X - radius, recRadii.Y - radius, radius));
	}

	static void DrawRectUv(Rectangle rec, Vector2 topLeftUv, Vector2 bottomLeftUv, Vector2 bottomRightUv, Vector2 topRightUv, Color tint, Vector3 normal)
	{
		Rlgl.Begin(DrawMode.Quads);
		{
			// set color and normal
			Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);
			Rlgl.Normal3f(normal.X, normal.Y, normal.Z);

			// todo: ceil the max position and floor the min position to ensure an acceptable size - adjust TexCoords accordingly

			// define all four vertices
			Rlgl.TexCoord2f(topLeftUv.X, topLeftUv.Y); // top left
			Rlgl.Vertex2f(rec.Position.X, rec.Position.Y);

			Rlgl.TexCoord2f(bottomLeftUv.X, bottomLeftUv.Y); // bottom left
			Rlgl.Vertex2f(rec.Position.X, rec.Position.Y + rec.Size.Y);

			Rlgl.TexCoord2f(bottomRightUv.X, bottomRightUv.Y); // bottom right
			Rlgl.Vertex2f(rec.Position.X + rec.Size.X, rec.Position.Y + rec.Size.Y);

			Rlgl.TexCoord2f(topRightUv.X, topRightUv.Y); // top right
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
		Color currentKeyColor = pressed? keyPressedColors[8] : keyColor;

		DrawRoundedRectangle(new(x, y + yOffset, spacebarWidth, keyBoxHeight), currentKeyColor, roundedBoxRadius);

		const int spacebarGlyphMarginX = 22;
		const int spacebarGlyphMarginY = 22;
		const int spacebarGlyphWidth = spacebarWidth - (2*spacebarGlyphMarginX);
		const int spacebarGlyphHeight = 20;
		const int spacebarGlyphThickness = 4;
		const int spacebarGlyphRadius = 12;

		DrawRoundedBaseRectangle(
			new(x + spacebarGlyphMarginX, y + yOffset + spacebarGlyphMarginY, spacebarGlyphWidth, spacebarGlyphHeight),
			backgroundColor, spacebarGlyphRadius);

		DrawRoundedBaseRectangle(
			new(x + spacebarGlyphMarginX + spacebarGlyphThickness, y + yOffset + spacebarGlyphMarginY, spacebarGlyphWidth - 2*spacebarGlyphThickness, spacebarGlyphHeight - spacebarGlyphThickness),
			currentKeyColor, spacebarGlyphRadius - spacebarGlyphThickness);
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