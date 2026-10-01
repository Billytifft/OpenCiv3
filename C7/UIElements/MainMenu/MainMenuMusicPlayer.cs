using Godot;
using System;
using Serilog;

public partial class MainMenuMusicPlayer : AudioStreamPlayer {
	private ILogger log;

	private bool musicEnabled = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		log = LogManager.ForContext<MainMenuMusicPlayer>();
		//Figured out how to load the mp3 from this post: https://godotengine.org/qa/30210/how-do-load-resource-works

		try {
			AudioStream stream = AudioLoader.Load("menu.main_menu_1");
			this.Stream = stream;

			int volume = AudioVolume.GetMusicVolume();
			musicEnabled = volume > 0;
			AudioVolume.ApplyMusicVolume(volume);

			if (musicEnabled) {
				log.Debug("setting music volume to {volume}% ({offset} decibel offset)", volume, AudioVolume.ToDecibelOffset(volume));
				Play();
			}
		} catch (ApplicationException ex) {
			log.Error(ex, "could not load mp3 for main menu music");
		}
	}
}
