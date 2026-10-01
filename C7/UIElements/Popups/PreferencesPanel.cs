using Godot;
using C7Engine;
using C7.UIElements;

// The Civ3-style preferences panel. Reachable from the main menu, the in-game
// menu, and Ctrl-P. Changes apply immediately and are written to C7.ini.
public partial class PreferencesPanel : Popup {
	private const int Width = 530;
	private const int Height = 250;
	private const int LeftMargin = 30;
	private const int FirstRow = 62;
	private const int RowHeight = 32;
	private const int SliderWidth = 150;
	private const int LabelWidth = 250;

	private int vOffset = FirstRow;

	public PreferencesPanel() {
		alignment = BoxContainer.AlignmentMode.Center;
		margins = new Margins(top: 100);
	}

	public override void _Ready() {
		base._Ready();

		AddTexture(Width, Height);
		AddBackground(Width, Height);
		AddHeader("Preferences", 10);

		AddGameSection();
		AddUnitsSection();
		AddAudioSection();

		AddCancelButton(new Vector2(Width - 40, Height - 40));
	}

	private void AddGameSection() {
		AddSectionHeader("Game");

		AddCheckbox("Ask which tech to research",
			C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch,
			defaultValue: true);
	}

	// No unit preferences exist yet. The section is kept so the layout matches
	// Civ3 and so future options have a home.
	private void AddUnitsSection() {
		AddSectionHeader("Units");
		vOffset += 8;
	}

	private void AddAudioSection() {
		AddSectionHeader("Audio");

		Civ3HSlider musicVolume = new() {
			MinValue = 0,
			MaxValue = 100,
			Step = 5,
			Value = AudioVolume.GetMusicVolume(),
		};
		Label readout = AddSliderRow("Music volume", musicVolume, musicVolume.Value);

		musicVolume.ValueChanged += (double value) => {
			AudioVolume.ApplyMusicVolume((int)value);
			readout.Text = $"{value:0}%";
		};
		musicVolume.DragEnded += (bool changed) => AudioVolume.SetMusicVolume((int)musicVolume.Value);
	}

	private void AddSectionHeader(string text) {
		Label label = MakeLabel(text, 18);
		label.AddThemeFontOverride("font", ResourceLoader.Load<FontFile>("res://Fonts/NSansFont24Pt.tres"));
		label.SetPosition(new Vector2(LeftMargin, vOffset));
		vOffset += 26;
	}

	private void AddCheckbox(string text, string section, string key, bool defaultValue) {
		Civ3Checkbox checkbox = new() {
			Text = text,
			// Civ3Checkbox only swaps to the pressed texture when ToggleMode is on.
			ToggleMode = true,
			ButtonPressed = C7Settings.GetBoolOrDefault(section, key, defaultValue),
		};
		checkbox.Toggled += (bool enabled) => {
			C7Settings.SetBool(section, key, enabled);
			C7Settings.SaveSettings();
		};
		checkbox.SetPosition(new Vector2(LeftMargin, vOffset));
		AddChild(checkbox);
		vOffset += RowHeight;
	}

	private Label AddSliderRow(string text, Civ3HSlider slider, double value) {
		MakeLabel(text, 14).SetPosition(new Vector2(LeftMargin, vOffset + 4));

		slider.CustomMinimumSize = new Vector2(SliderWidth, 20);
		slider.SetPosition(new Vector2(LeftMargin + LabelWidth, vOffset));
		AddChild(slider);

		Label readout = MakeLabel($"{value:0}%", 14);
		readout.SetPosition(new Vector2(LeftMargin + LabelWidth + SliderWidth + 10, vOffset + 4));

		vOffset += RowHeight;
		return readout;
	}

	private Label MakeLabel(string text, int fontSize) {
		Label label = new() {
			Text = text,
		};
		label.AddThemeFontSizeOverride("font_size", fontSize);
		AddChild(label);
		return label;
	}
}
