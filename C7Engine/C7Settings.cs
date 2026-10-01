using System;
using System.Collections.Generic;
using System.Linq;
using IniParser.Model;
using IniParser.Exceptions;

namespace C7Engine {
	public class C7Settings {
		private const string SETTINGS_FILE_NAME = "C7.ini";
		public static IniData settings;

		public static class LastGame {
			public const string SectionName = nameof(LastGame);
			public const string WorldSize = nameof(WorldSize);
			public const string BarbarianActivity = nameof(BarbarianActivity);
			public const string Landform = nameof(Landform);
			public const string OceanCoverage = nameof(OceanCoverage);
			public const string Climate = nameof(Climate);
			public const string Temperature = nameof(Temperature);
			public const string Age = nameof(Age);
			public const string Civilization = nameof(Civilization);
			public const string Difficulty = nameof(Difficulty);
			public const string Opponents = nameof(Opponents);
		}

		// Gameplay and interface preferences, as surfaced in the Preferences panel.
		public static class Preferences {
			public const string SectionName = nameof(Preferences);
			public const string PromptForResearch = nameof(PromptForResearch);
		}

		public static class Audio {
			// Lower case on purpose: ini-parser treats section and key names as
			// case-sensitive, and existing C7.ini files already use these names.
			public const string SectionName = "audio";
			public const string MusicVolume = "musicVolume";
			public const int DefaultMusicVolume = 100;
		}

		public static void LoadSettings() {
			try {
				settings = Util.GetFileIniDataParser().ReadFile(SETTINGS_FILE_NAME);
			} catch (ParsingException) {
				//First run.  The file doesn't exist.  That's okay.  We'll use sensible defaults.
				settings = new IniData();
				SaveSettings();
			}
		}

		public static void SaveSettings() {
			Util.GetFileIniDataParser().WriteFile(SETTINGS_FILE_NAME, settings);
		}

		public static void SetValue(string section, string key, string value) {
			if (settings == null) {
				LoadSettings();
			}
			settings[section][key] = value;
		}

		public static string GetSettingValue(string section, string key) {
			if (settings == null) {
				LoadSettings();
			}
			return settings[section][key];
		}

		public static string GetSettingsValueOrDefault(string section, string key, string defaultValue) {
			if (settings == null) {
				LoadSettings();
			}
			if (settings[section] == null) {
				return defaultValue;
			}
			if (settings[section][key] == null) {
				return defaultValue;
			}
			return settings[section][key];
		}

		public static T GetTypedSettingOrDefault<T>(string section, string key, T defaultValue) where T : struct, Enum {
			string value = GetSettingValue(section, key);
			return Enum.TryParse(value, true, out T result) ? result : defaultValue;
		}

		public static bool GetBoolOrDefault(string section, string key, bool defaultValue) {
			return GetSettingsValueOrDefault(section, key, defaultValue ? "true" : "false") == "true";
		}

		public static void SetBool(string section, string key, bool value) {
			SetValue(section, key, value ? "true" : "false");
		}

		public static int GetIntOrDefault(string section, string key, int defaultValue) {
			string value = GetSettingsValueOrDefault(section, key, defaultValue.ToString());
			return int.TryParse(value, out int result) ? result : defaultValue;
		}

		public static bool UseStandaloneMode() {
			return GetSettingsValueOrDefault("locations", "useStandaloneMode", "false") == "true";
		}
	}
}
