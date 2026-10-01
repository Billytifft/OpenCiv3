using System;
using C7Engine;
using IniParser.Model;
using Xunit;

namespace EngineTests.GameData;

public class C7SettingsTests : IDisposable {
	private readonly IniData originalSettings = C7Settings.settings;

	public C7SettingsTests() {
		// Swap in an empty in-memory ini so these tests never touch the real
		// C7.ini on disk. Never call SaveSettings() here.
		C7Settings.settings = new IniData();
	}

	public void Dispose() {
		C7Settings.settings = originalSettings;
	}

	[Fact]
	public void TestBoolDefaultsWhenKeyIsMissing() {
		Assert.True(C7Settings.GetBoolOrDefault(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, true));
		Assert.False(C7Settings.GetBoolOrDefault(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, false));
	}

	[Fact]
	public void TestBoolRoundTrip() {
		C7Settings.SetBool(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, false);
		Assert.False(C7Settings.GetBoolOrDefault(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, true));

		C7Settings.SetBool(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, true);
		Assert.True(C7Settings.GetBoolOrDefault(C7Settings.Preferences.SectionName, C7Settings.Preferences.PromptForResearch, false));
	}

	[Fact]
	public void TestIntDefaultsWhenKeyIsMissingOrMalformed() {
		Assert.Equal(100, C7Settings.GetIntOrDefault(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, 100));
		Assert.Equal(42, C7Settings.GetIntOrDefault(C7Settings.Audio.SectionName, "notAKey", 42));

		C7Settings.SetValue(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, "loud");
		Assert.Equal(100, C7Settings.GetIntOrDefault(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, 100));
	}

	[Fact]
	public void TestIntRoundTrip() {
		C7Settings.SetValue(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, "35");
		Assert.Equal(35, C7Settings.GetIntOrDefault(C7Settings.Audio.SectionName, C7Settings.Audio.MusicVolume, 100));
	}

	[Fact]
	public void TestSetValueCreatesMissingSection() {
		C7Settings.SetValue("brandNewSection", "key", "value");
		Assert.Equal("value", C7Settings.GetSettingsValueOrDefault("brandNewSection", "key", "fallback"));
	}
}
