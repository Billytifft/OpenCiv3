using System;
using Godot;
using C7Engine;

public static class AudioVolume {
	// There is no bus layout yet, so everything plays on Master. This moves to a
	// dedicated music bus when the audio manager lands.
	private const string MusicBus = "Master";

	/**
	 * Godot uses a decibel offset volume system, described at https://docs.godotengine.org/en/stable/tutorials/audio/audio_buses.html
	 * This is what audio professionals would use, but is not intuitive to end users.
	 * In this system, a 6db difference halves or doubles the volume.
	 * So our users are probably more used to a 0% to 100% system.
	 * So this method converts between them.
	 */
	public static float ToDecibelOffset(int percent) {
		if (percent <= 0) {
			return float.MinValue;
		}
		if (percent >= 100) {
			return 0;
		}
		//Conversion math based on https://stackoverflow.com/a/37810295/3534605
		return 20.0f * (float)(Math.Log10(percent / 100.0f));
	}

	public static int GetMusicVolume() {
		return C7Settings.GetIntOrDefault(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, C7Settings.Audio.DefaultMusicVolume);
	}

	public static void SetMusicVolume(int percent) {
		percent = Math.Clamp(percent, 0, 100);
		C7Settings.SetValue(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, percent.ToString());
		C7Settings.SaveSettings();
		ApplyMusicVolume(percent);
	}

	public static void ApplyMusicVolume(int percent) {
		int busIndex = AudioServer.GetBusIndex(MusicBus);
		if (busIndex < 0) {
			return;
		}
		AudioServer.SetBusVolumeDb(busIndex, ToDecibelOffset(percent));
		AudioServer.SetBusMute(busIndex, percent <= 0);
	}
}
